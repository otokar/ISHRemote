﻿Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Get-IshSetting"
try {

Describe “Get-IshSetting" -Tags "Read" {
	Context “Get-IshSetting ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Get-IshSetting -IShSession "INVALIDISHSESSION" -FieldName "NAME" -FilePath "INVALIDFILEPATH" } | Should Throw
		}
		It "Parameter FileName invalid" {
			{ Get-IshSetting -IShSession $ishSession -FieldName "INVALIDFIELDNAME" -FilePath "INVALIDFILEPATH" } | Should Throw
		}
		It "Parameter FilePath invalid" {
			{ Get-IshSetting -IShSession $ishSession -FieldName "NAME" -FilePath "INVALIDFILEPATH" } | Should Throw
		}
	}

	Context “Get-IshSetting ParameterGroup returns string object" {
		$fieldValue = Get-IshSetting -IShSession $ishSession -FieldName "NAME"
		It "GetType().Name" {
			$fieldValue.GetType().Name | Should BeExactly "String"
		}
		It "NAME is 'Configuration card'" {
			$fieldValue | Should Be "Configuration card"
		}
	}

	Context “Get-IshSetting ParameterGroup returns string FileInfo object" {
		# Create temp file path, but make sure it doesn't exist before calling Get-IshSetting
		$tempFilePath = (New-TemporaryFile).FullName
		Remove-Item $tempFilePath -Force
		$fileInfo = Get-IshSetting -IShSession $ishSession -FieldName "NAME" -FilePath $tempFilePath
		It "GetType().Name" {
			$fileInfo.GetType().Name | Should BeExactly "FileInfo"
		}
		It "File Exists" {
			$fileInfo.FullName -eq $tempFilePath | Should Be $true
			Test-Path -Path $fileInfo.FullName | Should Be $true
		}
		It "file NAME is 'Configuration card'" {
			(Get-Content $tempFilePath) | Should Be "Configuration card"
		}
	}

	Context “Get-IshSetting ParameterGroup Force returns xml FileInfo object" {
		# Create readonly temp file path, but make it gets overwritten with the -Force parameter
		$tempFilePath = (New-TemporaryFile).FullName
		Set-ItemProperty $tempFilePath -Name IsReadOnly -Value $true
		$fileInfo = Get-IshSetting -IShSession $ishSession -FieldName "FISHBACKGROUNDTASKCONFIG" -FilePath $tempFilePath -Force
		It "FISHBACKGROUNDTASKCONFIG is xml" {
			$fileContent = Get-Content $tempFilePath
			$fileContent -like "<infoShareBackgroundTaskConfig*" | Should Be $true
		}
	}

	Context "Get-IshSetting RequestedMetadataGroup returns IshFields object" {
		$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FINBOXCONFIGURATION" |
                             Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHBACKGROUNDTASKCONFIG" |
                             Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHCHANGETRACKERCONFIG" |
                             Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHENRICHURI" |
                             Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHEXTENSIONCONFIG" |
                             Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHLCURI" |
                             Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHPUBSTATECONFIG" |
                             Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHSYSTEMRESOLUTION" |
                             Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHTRANSJOBSTATECONFIG" |
                             Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHWRITEOBJPLUGINCFG" |
                             Set-IshRequestedMetadataField -IshSession $ishSession -Name "FSTATECONFIGURATION" |
                             Set-IshRequestedMetadataField -IshSession $ishSession -Name "FTRANSLATIONCONFIGURATION" |
                             Set-IshRequestedMetadataField -IshSession $ishSession -Name "MODIFIED-ON" |
                             Set-IshRequestedMetadataField -IshSession $ishSession -Name "NAME"
		$ishFields = Get-IshSetting -IshSession $ishSession -RequestedMetadata $requestedMetadata
		It "GetType().Name" {
			$ishFields.GetType().Name | Should BeExactly "Object[]"
		}
		It "ishFields.Length" {
			$ishFields.Length| Should Be 14
		}
		It "xml NAME is 'Configuration card'" {
			(Get-IshMetadataField -IshSession $ishSession -IshField $ishFields -Name "NAME" -Level None) | Should Be "Configuration card"
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	try { Remove-Item $tempFilePath -Force } catch { }
}
