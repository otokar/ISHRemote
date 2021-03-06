/*
* Copyright (c) 2014 All Rights Reserved by the SDL Group.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*     http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.Xml;
using System.Xml.Linq;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;

namespace Trisoft.ISHRemote.Cmdlets.Settings
{
    /// <summary>
    /// <para type="synopsis">This cmdlet can be used to get a configuration setting from the repository. Depending on the parameters you use, the setting will be returned differently.
    /// If you provide:
    /// * A requested metadata parameter with the fields to get, the setting will be return as IshFields
    /// * A fieldname and no filepath, the setting will be returned as string
    /// * A fieldname and a filepath, the setting will be saved to the file. 
    ///   If the file is already present, providing -Force will allow you to overwrite the file</para>
    /// <para type="description">This cmdlet can be used to get a configuration setting from the repository. Depending on the parameters you use, the setting will be returned differently.
    /// If you provide:
    /// * A requested metadata parameter with the fields to get, the setting will be return as IshFields
    /// * A fieldname and no filepath, the setting will be returned as string
    /// * A fieldname and a filepath, the setting will be saved to the file. 
    ///   If the file is already present, providing -Force will allow you to overwrite the file</para>
    /// </summary>
    /// <example>
    /// <code>
    /// Param(
    ///     $wsBaseUrl = 'https://example.com/InfoShareWS/',
    ///     $userName = 'admin',
    ///     $password = 'admin',
    ///     $settingsFolderPath = 'D:\temp'
    /// )
    /// $ishSession = New-IshSession -WsBaseUrl $wsBaseUrl -IshUserName $userName -IshPassword $password
    /// $settingsFolderPath = Join-Path -Path $settingsFolderPath -ChildPath ((Get-Date -Format yyyyMMddHHmmss) + "=" + $wsBaseUrl.Replace(":","-").Replace("/","-") + "=" + (Get-IshVersion -IshSession $ishSession).ToString())
    /// Write-Verbose "Saving in $settingsFolderPath"
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLInboxConfiguration.xml"
    /// Get-IshSetting -IshSession $ishSession -FieldName "FINBOXCONFIGURATION" -FilePath $filePath
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLBackgroundTaskConfiguration.xml"
    /// Get-IshSetting -IshSession $ishSession -FieldName "FISHBACKGROUNDTASKCONFIG" -FilePath $filePath
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLChangeTrackerConfig.xml"
    /// Get-IshSetting -IshSession $ishSession -FieldName "FISHCHANGETRACKERCONFIG" -FilePath $filePath
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLExtensionConfiguration.xml"
    /// Get-IshSetting -IshSession $ishSession -FieldName "FISHEXTENSIONCONFIG" -FilePath $filePath
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLPluginConfig.xml"
    /// Get-IshSetting -IshSession $ishSession -FieldName "FISHPLUGINCONFIGXML" -FilePath $filePath
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLStatusConfiguration.xml"
    /// Get-IshSetting -IshSession $ishSession -FieldName "FSTATECONFIGURATION" -FilePath $filePath
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLTranslationConfiguration.xml"
    /// Get-IshSetting -IshSession $ishSession -FieldName "FTRANSLATIONCONFIGURATION" -FilePath $filePath
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLWriteObjPluginConfig.xml"
    /// Get-IshSetting -IshSession $ishSession -FieldName "FISHWRITEOBJPLUGINCFG" -FilePath $filePath
    /// $filePath = Join-Path -Path $settingsFolderPath -ChildPath "Admin.XMLPublishPluginConfiguration.xml"
    /// Get-IshSetting -IshSession $ishSession -FieldName "FISHPUBLISHPLUGINCONFIG" -FilePath $filePath
    /// Write-Host "Done, see $settingsFolderPath"
    /// </code>
    /// <para>Retrieve all Settings xml configuration entries and save them in a folder to desk allowing file-to-file comparison with EnterViaUI folder.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshSetting", SupportsShouldProcess = false)]
    [OutputType(typeof(IshField),typeof(FileInfo),typeof(string))]
    public sealed class GetIshSetting : SettingsCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "RequestedMetadataGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The metadata fields to retrieve</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "RequestedMetadataGroup")]
        [ValidateNotNull]
        public IshField[] RequestedMetadata { get; set; }

        /// <summary>
        /// <para type="description">The settings field to retrieve</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public string FieldName { get; set; }

        /// <summary>
        /// <para type="description">File on the Windows filesystem where to save the retrieved setting</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public string FilePath { get; set; }

        /// <summary>
        /// <para type="description">When set, will override the file when it already exists</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter Force { get; set; }


        protected override void ProcessRecord()
        {
            try
            {

                // 2. Retrieve the updated material from the database and write it out
                WriteDebug("Retrieving");

                IshFields requestedMetadata = new IshFields();
                if (RequestedMetadata != null)
                {
                    requestedMetadata = new IshFields(RequestedMetadata);
                }
                else if (FieldName != null)
                {
                    requestedMetadata.AddField(new IshRequestedMetadataField(FieldName, Enumerations.Level.None, Enumerations.ValueType.Value));
                }

                var metadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(ISHType, requestedMetadata, Enumerations.ActionMode.Read);
                string xmlIshObjects = IshSession.Settings25.GetMetadata(metadata.ToXml());
                var ishFields = new IshObjects(xmlIshObjects).Objects[0].IshFields;
                if (RequestedMetadata != null)
                {
                    // 3. Write it
                    WriteVerbose("returned object count[1]");
                    WriteObject(ishFields.Fields(), true);
                }
                else if (FieldName != null)
                {
                    if (FilePath != null)
                    {
                        //Create the file.
                        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                        var fileMode = (Force.IsPresent) ? FileMode.Create : FileMode.CreateNew;

                        string value = ishFields.GetFieldValue(FieldName, Enumerations.Level.None, Enumerations.ValueType.Value);
                        if (!String.IsNullOrEmpty(value))
                        {
                            try
                            {
                                // Let's try to see if it is xml first
                                var doc = XDocument.Parse(value);
                                XmlWriterSettings settings = new XmlWriterSettings();
                                settings.Indent = true;
                                using (var stream = new FileStream(FilePath, fileMode, FileAccess.Write))
                                {
                                    using (XmlWriter xmlWriter = XmlWriter.Create(stream, settings))
                                    {
                                        doc.Save(xmlWriter);
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // remove potential readonly flag
                                if (File.Exists(FilePath))
                                { 
                                    File.SetAttributes(FilePath, FileAttributes.Normal);
                                }
                                // Write it as a text file
                                using (var stream = new FileStream(FilePath, fileMode, FileAccess.ReadWrite))
                                {
                                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(stream, Encoding.UTF8))
                                    {
                                        writer.Write(value);
                                    }
                                }
                            }
                        }                        
                        WriteVerbose("returned object count[1]");
                        WriteObject(new FileInfo(FilePath));
                    }
                    else
                    {
                        WriteVerbose("returned object count[1]");
                        WriteObject(ishFields.GetFieldValue(FieldName, Enumerations.Level.None, Enumerations.ValueType.Value));
                    }
                }
            }
            catch (TrisoftAutomationException trisoftAutomationException)
            {
                ThrowTerminatingError(new ErrorRecord(trisoftAutomationException, base.GetType().Name, ErrorCategory.InvalidOperation, null));
            }
            catch (Exception exception)
            {
                ThrowTerminatingError(new ErrorRecord(exception, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
        }
    }
}

