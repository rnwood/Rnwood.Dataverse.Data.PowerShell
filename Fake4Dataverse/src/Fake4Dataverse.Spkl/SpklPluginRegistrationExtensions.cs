using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fake4Dataverse.Pipeline;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Spkl
{
    /// <summary>
    /// Represents the result of auto-registering SPKL-decorated plugin steps.
    /// Dispose to unregister all created registrations.
    /// </summary>
    public sealed class SpklAutoRegistrationResult : IDisposable
    {
        private readonly List<PipelineStepRegistration> _registrations;
        private bool _disposed;

        internal SpklAutoRegistrationResult(List<PipelineStepRegistration> registrations, List<SpklSkippedRegistration> skippedRegistrations)
        {
            _registrations = registrations;
            Registrations = registrations.AsReadOnly();
            SkippedRegistrations = skippedRegistrations.AsReadOnly();
        }

        /// <summary>
        /// Gets the successfully created pipeline registrations.
        /// </summary>
        public IReadOnlyList<PipelineStepRegistration> Registrations { get; }

        /// <summary>
        /// Gets registrations that were intentionally skipped (for example unsupported attribute forms).
        /// </summary>
        public IReadOnlyList<SpklSkippedRegistration> SkippedRegistrations { get; }

        /// <summary>
        /// Unregisters all successful registrations created by this result.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            foreach (var registration in _registrations)
            {
                registration.Dispose();
            }
        }
    }

    /// <summary>
    /// Describes a SPKL attribute that was discovered but skipped.
    /// </summary>
    public sealed class SpklSkippedRegistration
    {
        internal SpklSkippedRegistration(Type pluginType, CrmPluginRegistrationAttribute attribute, string reason)
        {
            PluginType = pluginType;
            Attribute = attribute;
            Reason = reason;
        }

        /// <summary>
        /// Gets the plugin type where the attribute was found.
        /// </summary>
        public Type PluginType { get; }

        /// <summary>
        /// Gets the discovered attribute instance.
        /// </summary>
        public CrmPluginRegistrationAttribute Attribute { get; }

        /// <summary>
        /// Gets the reason why this attribute was skipped.
        /// </summary>
        public string Reason { get; }
    }

    /// <summary>
    /// Extension methods for auto-registering plugins decorated with
    /// <see cref="CrmPluginRegistrationAttribute"/> into <see cref="FakeDataverseEnvironment.Pipeline"/>.
    /// </summary>
    public static class SpklPluginRegistrationExtensions
    {
        /// <summary>
        /// Scans an assembly for non-abstract <see cref="IPlugin"/> types decorated with
        /// <see cref="CrmPluginRegistrationAttribute"/>, and registers supported plugin-step forms.
        /// Unsupported forms (workflow/custom API) are skipped and reported in the result.
        /// </summary>
        /// <param name="environment">Target fake Dataverse environment.</param>
        /// <param name="assembly">Assembly to scan.</param>
        /// <returns>A disposable result containing successful and skipped registrations.</returns>
        public static SpklAutoRegistrationResult RegisterSpklPluginsFromAssembly(this FakeDataverseEnvironment environment, Assembly assembly)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            var pluginTypes = GetLoadableTypes(assembly)
                .Where(t => t != null)
                .Where(t => typeof(IPlugin).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .OrderBy(t => t.FullName, StringComparer.Ordinal)
                .ToArray();

            return RegisterSpklPlugins(environment, pluginTypes);
        }

        /// <summary>
        /// Registers SPKL-decorated plugin types.
        /// Unsupported forms (workflow/custom API) are skipped and reported in the result.
        /// </summary>
        /// <param name="environment">Target fake Dataverse environment.</param>
        /// <param name="pluginTypes">Plugin types to inspect.</param>
        /// <returns>A disposable result containing successful and skipped registrations.</returns>
        public static SpklAutoRegistrationResult RegisterSpklPlugins(this FakeDataverseEnvironment environment, params Type[] pluginTypes)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (pluginTypes == null) throw new ArgumentNullException(nameof(pluginTypes));

            return RegisterSpklPlugins(environment, (IEnumerable<Type>)pluginTypes);
        }

        /// <summary>
        /// Registers SPKL-decorated plugin types.
        /// Unsupported forms (workflow/custom API) are skipped and reported in the result.
        /// </summary>
        /// <param name="environment">Target fake Dataverse environment.</param>
        /// <param name="pluginTypes">Plugin types to inspect.</param>
        /// <returns>A disposable result containing successful and skipped registrations.</returns>
        public static SpklAutoRegistrationResult RegisterSpklPlugins(this FakeDataverseEnvironment environment, IEnumerable<Type> pluginTypes)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (pluginTypes == null) throw new ArgumentNullException(nameof(pluginTypes));

            var registrations = new List<PipelineStepRegistration>();
            var skipped = new List<SpklSkippedRegistration>();

            foreach (var pluginType in pluginTypes.Where(t => t != null).OrderBy(t => t.FullName, StringComparer.Ordinal))
            {
                // Match by full type name so that plugins that embedded the spkl
                // content file directly (a different CLR type with the same name)
                // are also discovered and mapped to our local CrmPluginRegistrationAttribute.
                var attributes = pluginType
                    .GetCustomAttributes(inherit: false)
                    .OfType<Attribute>()
                    .Select(MapToCrmPluginRegistrationAttribute)
                    .Where(a => a != null)
                    .Cast<CrmPluginRegistrationAttribute>()
                    .ToArray();

                if (attributes.Length == 0)
                {
                    continue;
                }

                foreach (var attribute in attributes)
                {
                    string reason;
                    PipelineStepRegistration? registration;
                    if (!TryRegisterAttribute(environment, pluginType, attribute, out registration, out reason))
                    {
                        skipped.Add(new SpklSkippedRegistration(pluginType, attribute, reason));
                        continue;
                    }

                    registrations.Add(registration!);
                }
            }

            return new SpklAutoRegistrationResult(registrations, skipped);
        }

        private static bool TryRegisterAttribute(
            FakeDataverseEnvironment environment,
            Type pluginType,
            CrmPluginRegistrationAttribute attribute,
            out PipelineStepRegistration? registration,
            out string reason)
        {
            registration = null;
            reason = string.Empty;

            if (!typeof(IPlugin).IsAssignableFrom(pluginType) || pluginType.IsAbstract || pluginType.IsInterface)
            {
                reason = "Type is not a concrete IPlugin implementation.";
                return false;
            }

            if (!attribute.Stage.HasValue)
            {
                reason = IsCustomApiForm(attribute)
                    ? "Custom API style [CrmPluginRegistration] is currently unsupported and was skipped."
                    : "Workflow activity style [CrmPluginRegistration] is currently unsupported and was skipped.";
                return false;
            }

            var messageName = attribute.Message;
            if (messageName == null || messageName.Trim().Length == 0)
            {
                reason = "Plugin-step registration is missing Message and was skipped.";
                return false;
            }

            PipelineStage pipelineStage;
            try
            {
                pipelineStage = MapStage(attribute.Stage.Value);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                reason = ex.Message;
                return false;
            }

            IPlugin plugin;
            if (!TryCreatePluginInstance(pluginType, attribute, out plugin, out reason))
            {
                return false;
            }

            var filteringAttributes = NormalizeCommaSeparatedValues(attribute.FilteringAttributes);
            if (filteringAttributes.Length > 0)
            {
                plugin = new FilteringAttributesPlugin(plugin, filteringAttributes);
            }

            var normalizedEntity = NormalizeEntityLogicalName(attribute.EntityLogicalName);
            registration = environment.Pipeline.RegisterStep(messageName, pipelineStage, normalizedEntity, plugin);

            if (attribute.ExecutionMode == ExecutionModeEnum.Asynchronous)
            {
                registration.SetAsynchronous();
            }

            ApplyImageMapping(registration, attribute.Image1Name, attribute.Image1Type, attribute.Image1Attributes);
            ApplyImageMapping(registration, attribute.Image2Name, attribute.Image2Type, attribute.Image2Attributes);

            return true;
        }

        private static bool TryCreatePluginInstance(
            Type pluginType,
            CrmPluginRegistrationAttribute attribute,
            out IPlugin plugin,
            out string reason)
        {
            plugin = null!;
            reason = string.Empty;

            if (pluginType.ContainsGenericParameters)
            {
                reason = "Open generic plugin types are unsupported.";
                return false;
            }

            var unsecure = attribute.UnSecureConfiguration ?? string.Empty;
            var secure = attribute.SecureConfiguration ?? string.Empty;

            try
            {
                var twoParamCtor = GetConstructor(pluginType, typeof(string), typeof(string));
                if (twoParamCtor != null)
                {
                    plugin = (IPlugin)twoParamCtor.Invoke(new object[] { unsecure, secure });
                    return true;
                }

                var oneParamCtor = GetConstructor(pluginType, typeof(string));
                if (oneParamCtor != null)
                {
                    plugin = (IPlugin)oneParamCtor.Invoke(new object[] { unsecure });
                    return true;
                }

                plugin = (IPlugin)Activator.CreateInstance(pluginType, nonPublic: true)!;
                return true;
            }
            catch (Exception ex)
            {
                reason = "Failed to construct plugin type '" + pluginType.FullName + "': " + ex.GetBaseException().Message;
                return false;
            }
        }

        private static ConstructorInfo? GetConstructor(Type type, params Type[] parameters)
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return type.GetConstructor(Flags, binder: null, types: parameters, modifiers: null);
        }

        // Returns our own CrmPluginRegistrationAttribute for:
        //  - attributes that already ARE our type (same CLR type), and
        //  - attributes from a foreign assembly whose type name is "CrmPluginRegistrationAttribute"
        //    (i.e., the plugin embedded the spkl content file directly).
        private static CrmPluginRegistrationAttribute? MapToCrmPluginRegistrationAttribute(Attribute attr)
        {
            if (attr is CrmPluginRegistrationAttribute ours)
            {
                return ours;
            }

            var t = attr.GetType();
            if (t.FullName != "CrmPluginRegistrationAttribute")
            {
                return null;
            }

            return MapForeignAttribute(t, attr);
        }

        // Reads all relevant properties from a foreign CrmPluginRegistrationAttribute via
        // reflection and constructs an equivalent local instance.
        private static CrmPluginRegistrationAttribute MapForeignAttribute(Type t, object attr)
        {
            var message = (string?)t.GetProperty("Message")?.GetValue(attr);
            var name = (string?)t.GetProperty("Name")?.GetValue(attr);
            var entityLogicalName = (string?)t.GetProperty("EntityLogicalName")?.GetValue(attr);
            var stageRaw = t.GetProperty("Stage")?.GetValue(attr);
            var execModeRaw = t.GetProperty("ExecutionMode")?.GetValue(attr);
            var filteringAttributes = (string?)t.GetProperty("FilteringAttributes")?.GetValue(attr);
            var executionOrder = Convert.ToInt32(t.GetProperty("ExecutionOrder")?.GetValue(attr) ?? 0);
            var isoModeRaw = t.GetProperty("IsolationMode")?.GetValue(attr);

            var isoMode = (IsolationModeEnum)Convert.ToInt32(isoModeRaw ?? IsolationModeEnum.Sandbox);

            CrmPluginRegistrationAttribute result;
            if (stageRaw != null)
            {
                // Plugin-step constructor form
                result = new CrmPluginRegistrationAttribute(
                    message ?? string.Empty,
                    entityLogicalName ?? string.Empty,
                    (StageEnum)Convert.ToInt32(stageRaw),
                    (ExecutionModeEnum)Convert.ToInt32(execModeRaw ?? ExecutionModeEnum.Synchronous),
                    filteringAttributes ?? string.Empty,
                    name ?? string.Empty,
                    executionOrder,
                    isoMode);
            }
            else if (name == null)
            {
                // Custom API constructor form: CrmPluginRegistrationAttribute(string message)
                result = new CrmPluginRegistrationAttribute(message ?? string.Empty);
            }
            else
            {
                // Workflow constructor form
                var friendlyName = (string?)t.GetProperty("FriendlyName")?.GetValue(attr);
                var description = (string?)t.GetProperty("Description")?.GetValue(attr);
                var groupName = (string?)t.GetProperty("GroupName")?.GetValue(attr);
                result = new CrmPluginRegistrationAttribute(
                    name,
                    friendlyName ?? string.Empty,
                    description ?? string.Empty,
                    groupName ?? string.Empty,
                    isoMode);
            }

            result.Id = (string?)t.GetProperty("Id")?.GetValue(attr);
            result.Image1Name = (string?)t.GetProperty("Image1Name")?.GetValue(attr);
            result.Image1Attributes = (string?)t.GetProperty("Image1Attributes")?.GetValue(attr);
            var image1TypeRaw = t.GetProperty("Image1Type")?.GetValue(attr);
            if (image1TypeRaw != null) result.Image1Type = (ImageTypeEnum)Convert.ToInt32(image1TypeRaw);
            result.Image2Name = (string?)t.GetProperty("Image2Name")?.GetValue(attr);
            result.Image2Attributes = (string?)t.GetProperty("Image2Attributes")?.GetValue(attr);
            var image2TypeRaw = t.GetProperty("Image2Type")?.GetValue(attr);
            if (image2TypeRaw != null) result.Image2Type = (ImageTypeEnum)Convert.ToInt32(image2TypeRaw);
            result.Description = (string?)t.GetProperty("Description")?.GetValue(attr);
            result.DeleteAsyncOperation = (bool)(t.GetProperty("DeleteAsyncOperation")?.GetValue(attr) ?? false);
            result.UnSecureConfiguration = (string?)t.GetProperty("UnSecureConfiguration")?.GetValue(attr);
            result.SecureConfiguration = (string?)t.GetProperty("SecureConfiguration")?.GetValue(attr);
            result.Offline = (bool)(t.GetProperty("Offline")?.GetValue(attr) ?? false);
            result.Server = (bool)(t.GetProperty("Server")?.GetValue(attr) ?? true);
            var actionRaw = t.GetProperty("Action")?.GetValue(attr);
            if (actionRaw != null) result.Action = (PluginStepOperationEnum)Convert.ToInt32(actionRaw);

            return result;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null)!;
            }
        }

        private static bool IsCustomApiForm(CrmPluginRegistrationAttribute attribute)
        {
            return !string.IsNullOrWhiteSpace(attribute.Message) && string.IsNullOrWhiteSpace(attribute.Name);
        }

        private static PipelineStage MapStage(StageEnum stage)
        {
            switch (stage)
            {
                case StageEnum.PreValidation:
                    return PipelineStage.PreValidation;
                case StageEnum.PreOperation:
                    return PipelineStage.PreOperation;
                case StageEnum.PostOperation:
                    return PipelineStage.PostOperation;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stage), stage, "Unsupported stage value.");
            }
        }

        private static string? NormalizeEntityLogicalName(string? entityLogicalName)
        {
            if (entityLogicalName == null)
            {
                return null;
            }

            var normalized = entityLogicalName.Trim();
            if (normalized.Length == 0)
            {
                return null;
            }

            if (string.Equals(normalized, "none", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return normalized;
        }

        private static void ApplyImageMapping(
            PipelineStepRegistration registration,
            string? imageName,
            ImageTypeEnum imageType,
            string? attributes)
        {
            if (imageName == null)
            {
                return;
            }

            var alias = imageName.Trim();
            if (alias.Length == 0)
            {
                return;
            }

            var normalizedAttributes = NormalizeCommaSeparatedValues(attributes);

            switch (imageType)
            {
                case ImageTypeEnum.PreImage:
                    registration.AddPreImage(alias, normalizedAttributes);
                    break;
                case ImageTypeEnum.PostImage:
                    registration.AddPostImage(alias, normalizedAttributes);
                    break;
                case ImageTypeEnum.Both:
                    registration.AddPreImage(alias, normalizedAttributes);
                    registration.AddPostImage(alias, normalizedAttributes);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(imageType), imageType, "Unsupported image type value.");
            }
        }

        private static string[] NormalizeCommaSeparatedValues(string? value)
        {
            if (value == null)
            {
                return Array.Empty<string>();
            }

            var trimmed = value.Trim();
            if (trimmed.Length == 0)
            {
                return Array.Empty<string>();
            }

            return trimmed
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim())
                .Where(v => v.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private sealed class FilteringAttributesPlugin : IPlugin
        {
            private readonly IPlugin _inner;
            private readonly HashSet<string> _filteringAttributes;

            public FilteringAttributesPlugin(IPlugin inner, IEnumerable<string> filteringAttributes)
            {
                _inner = inner;
                _filteringAttributes = new HashSet<string>(filteringAttributes, StringComparer.OrdinalIgnoreCase);
            }

            public void Execute(IServiceProvider serviceProvider)
            {
                var context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
                if (ShouldExecute(context))
                {
                    _inner.Execute(serviceProvider);
                }
            }

            private bool ShouldExecute(IPluginExecutionContext? context)
            {
                if (_filteringAttributes.Count == 0)
                {
                    return true;
                }

                if (context == null)
                {
                    return true;
                }

                if (!string.Equals(context.MessageName, "Update", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (!context.InputParameters.Contains("Target"))
                {
                    return false;
                }

                var target = context.InputParameters["Target"] as Entity;
                if (target == null)
                {
                    return false;
                }

                foreach (var key in target.Attributes.Keys)
                {
                    if (_filteringAttributes.Contains(key))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}