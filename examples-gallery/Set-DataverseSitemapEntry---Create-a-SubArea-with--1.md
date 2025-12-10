---
title: "Set-DataverseSitemapEntry - Create a SubArea with entity reference"
tags: ['Metadata']
source: "Set-DataverseSitemapEntry.md"
---
Creates a new SubArea positioned before the "ContactsSubarea" entry.

```powershell
$titles = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
$titles.Add(1033, "Accounts")
Set-DataverseSitemapEntry -SitemapUniqueName "MySitemap" -SubArea -EntryId "AccountsSubarea" `
   -ParentAreaId "SalesArea" -ParentGroupId "CustomersGroup" `
   -Entity "account" -Titles $titles -Before "ContactsSubarea"

```
