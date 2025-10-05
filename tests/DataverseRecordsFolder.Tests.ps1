. $PSScriptRoot/Common.ps1

Describe 'Set-DataverseRecordsFolder and Get-DataverseRecordsFolder' {

    It 'Writes PSObjects to files named by Id and Get reads them back' {
        $tmp = Join-Path -Path ([IO.Path]::GetTempPath()) -ChildPath ([Guid]::NewGuid().ToString())
        New-Item -ItemType Directory -Path $tmp | Out-Null

        try {
            $id = [Guid]::NewGuid()
            $obj = [PSCustomObject]@{
                Id = $id
                firstname = 'FileTest'
                number = 123
                when = [datetime]::Parse('2020-01-01T12:34:56Z')
                nested = @{ a = 1; b = 'x' }
            }

            $obj | Set-DataverseRecordsFolder -OutputPath $tmp

            $file = Join-Path $tmp ("$($id).json")
            Test-Path $file | Should -BeTrue

            $read = Get-DataverseRecordsFolder -InputPath $tmp | Where-Object { $_.Id -eq $id }
            $read | Should -Not -BeNull
            $read.firstname | Should -Be 'FileTest'
            $read.number | Should -Be 123
            # JSON serialisation may convert datetime to string; ensure round-trip value is present
            $read.when | Should -Not -BeNull
            $read.nested.a | Should -Be 1
        }
        finally {
            if (Test-Path $tmp) { Remove-Item -Recurse -Force $tmp }
        }
    }

    It 'Respects idproperties and writes files with composite names' {
        $tmp = Join-Path -Path ([IO.Path]::GetTempPath()) -ChildPath ([Guid]::NewGuid().ToString())
        New-Item -ItemType Directory -Path $tmp | Out-Null

        try {
            $obj = [PSCustomObject]@{
                firstname = 'Alice'
                lastname = 'Jones'
                Id = [Guid]::NewGuid()
            }

            $obj | Set-DataverseRecordsFolder -OutputPath $tmp -idproperties @('firstname','lastname')

            $expected = Join-Path $tmp 'Alice-Jones.json'
            Test-Path $expected | Should -BeTrue
        }
        finally {
            if (Test-Path $tmp) { Remove-Item -Recurse -Force $tmp }
        }
    }

    It 'Throws when generated filenames are not unique' {
        $tmp = Join-Path -Path ([IO.Path]::GetTempPath()) -ChildPath ([Guid]::NewGuid().ToString())
        New-Item -ItemType Directory -Path $tmp | Out-Null

        try {
            $o1 = [PSCustomObject]@{ Id = 'same'; firstname = 'A' }
            $o2 = [PSCustomObject]@{ Id = 'same'; firstname = 'B' }

            # Both objects have same Id -> same filename -> error expected
            { @($o1,$o2) | Set-DataverseRecordsFolder -OutputPath $tmp } | Should -Throw
        }
        finally {
            if (Test-Path $tmp) { Remove-Item -Recurse -Force $tmp }
        }
    }

    It 'Moves deleted files into deletions folder when withdeletions is specified and creates keep.me when deletions empty' {
        $tmp = Join-Path -Path ([IO.Path]::GetTempPath()) -ChildPath ([Guid]::NewGuid().ToString())
        New-Item -ItemType Directory -Path $tmp | Out-Null

        try {
            $id1 = [Guid]::NewGuid().ToString()
            $id2 = [Guid]::NewGuid().ToString()

            $o1 = [PSCustomObject]@{ Id = $id1; firstname = 'One' }
            $o2 = [PSCustomObject]@{ Id = $id2; firstname = 'Two' }

            # Initial write with both
            @($o1,$o2) | Set-DataverseRecordsFolder -OutputPath $tmp
            Test-Path (Join-Path $tmp "$id1.json") | Should -BeTrue
            Test-Path (Join-Path $tmp "$id2.json") | Should -BeTrue

            # Now write only o1 with deletions enabled: o2 should be moved to deletions
            $o1 | Set-DataverseRecordsFolder -OutputPath $tmp -withdeletions
            Test-Path (Join-Path $tmp "deletions\$id2.json") | Should -BeTrue

            # Now recreate both; deleted file should be removed from deletions and keep.me created (deletions empty)
            @($o1,$o2) | Set-DataverseRecordsFolder -OutputPath $tmp -withdeletions
            Test-Path (Join-Path $tmp "deletions\$id2.json") | Should -BeFalse
            Test-Path (Join-Path $tmp "deletions\keep.me") | Should -BeTrue
        }
        finally {
            if (Test-Path $tmp) { Remove-Item -Recurse -Force $tmp }
        }
    }

    It 'Get-DataverseRecordsFolder reads deletions when -deletions switch specified' {
        $tmp = Join-Path -Path ([IO.Path]::GetTempPath()) -ChildPath ([Guid]::NewGuid().ToString())
        New-Item -ItemType Directory -Path $tmp | Out-Null
        New-Item -ItemType Directory -Path (Join-Path $tmp 'deletions') | Out-Null

        try {
            $id = [Guid]::NewGuid().ToString()
            $content = @{ Id = $id; firstname = 'Del' } | ConvertTo-Json -Depth 5
            $file = Join-Path $tmp 'deletions'
            $filePath = Join-Path $file ("$id.json")
            $content | Out-File -Encoding utf8 -FilePath $filePath

            $read = Get-DataverseRecordsFolder -InputPath $tmp -deletions
            $read | Where-Object { $_.Id -eq $id } | Should -Not -BeNull
        }
        finally {
            if (Test-Path $tmp) { Remove-Item -Recurse -Force $tmp }
        }
    }

}
