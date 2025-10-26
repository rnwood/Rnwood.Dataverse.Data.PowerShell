Describe "Compare-DataverseSolution" {
    BeforeAll {
        $modulePath = $env:TESTMODULEPATH
        Import-Module $modulePath -Force
    }

    Context "Parameter validation" {
        It "Should require a parameter set to be specified" {
            { Compare-DataverseSolution } | Should -Throw -ErrorId "ParameterSetNotFound,Compare-DataverseSolution"
        }

        It "Should error when -FileToEnvironment is used without SolutionFile" {
            { Compare-DataverseSolution -FileToEnvironment } | Should -Throw -ErrorId "MissingMandatoryParameter,Compare-DataverseSolution"
        }

        It "Should error when -BytesToEnvironment is used without SolutionBytes" {
            { Compare-DataverseSolution -BytesToEnvironment } | Should -Throw -ErrorId "MissingMandatoryParameter,Compare-DataverseSolution"
        }

        It "Should error when -FileToFile is used without SolutionFile" {
            { Compare-DataverseSolution -FileToFile -TargetSolutionFile "dummy.zip" } | Should -Throw -ErrorId "MissingMandatoryParameter,Compare-DataverseSolution"
        }

        It "Should error when -FileToFile is used without TargetSolutionFile" {
            { Compare-DataverseSolution -FileToFile -SolutionFile "dummy.zip" } | Should -Throw -ErrorId "MissingMandatoryParameter,Compare-DataverseSolution"
        }
    }

    # Additional tests for functionality would require mock solution files and environment
    # For now, parameter validation is tested
}