﻿/*
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
using System.Linq;
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;

namespace Trisoft.ISHRemote.Cmdlets.Baseline
{
    /// <summary>
    /// <para type="synopsis">The Add-IshBaseline cmdlet adds the new baselines that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Add-IshBaseline cmdlet adds the new baselines that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// Add-IshBaseline -IshSession $ishSession -Name "My first baseline"
    /// </code>
    /// <para>Create a baseline ishobject, that holds no baseline entries</para>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "IshBaseline", SupportsShouldProcess = true)]
    [OutputType(typeof(IshObject))]
    public sealed class AddIshBaseline : BaselineCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Array with the baselines to create. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// <para type="description">The name of the new baseline.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// API function takes user group parameter, in practice it is not used, so forcing empty UserGroup
        /// </summary>
        private string UserGroup { get { return string.Empty; } }

        protected override void ProcessRecord()
        {
            try
            {
                List<IshObject> returnedObjects = new List<IshObject>();

                if (IshObject != null && IshObject.Length == 0)
                {
                    // Do nothing
                    WriteVerbose("IshObject is empty, so nothing to create");
                    WriteVerbose("IshObject is empty, so nothing to retrieve");
                }
                else
                {
                    List<string> returnBaselines = new List<string>();
                    IshFields returnFields;

                    // 1. Doing the update
                    WriteDebug("Adding");

                    if (IshObject != null)
                    {
                        int current = 0;
                        // 1b. Using IshObject[] pipeline or specificly set
                        foreach (IshObject ishObject in IshObject)
                        {
                            IshMetadataField baselineNameValueField =
                                (IshMetadataField)
                                    ishObject.IshFields.Retrieve(FieldElements.BaselineName, Enumerations.Level.None,
                                        Enumerations.ValueType.Value)[0];
                            string baselineName = baselineNameValueField.Value;
                            WriteDebug($"BaselineName[{baselineName}] Metadata.length[{ishObject.IshFields.ToXml().Length}] {++current}/{IshObject.Length}");
                            if (ShouldProcess(baselineName))
                            {
                                var baselineId = IshSession.Baseline25.Create(baselineName, string.Empty);
                                returnBaselines.Add(baselineId);
                            }
                        }
                        returnFields = (IshObject[0] == null)
                            ? new IshFields()
                            : IshObject[0].IshFields;
                    }
                    else
                    {
                        // 1a. Using Id and Metadata
                        WriteVerbose($"BaselineName[{Name}]");
                        if (ShouldProcess(Name))
                        {
                            var baselineId = IshSession.Baseline25.Create(Name, UserGroup);
                            returnBaselines.Add(baselineId);
                        }
                        returnFields = new IshFields();
                    }

                    // 2. Retrieve the updated material from the database and write it out
                    WriteDebug("Retrieving");
                    // 2a. Prepare list of baslineIds and requestedmetadata
                    // 2b. Retrieve the material

                    // Add the required fields (needed for pipe operations)
                    IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(ISHType, returnFields, Enumerations.ActionMode.Read);
                    string xmlIshObjects = IshSession.Baseline25.RetrieveMetadata(
                        returnBaselines.ToArray(),
                        "",
                        requestedMetadata.ToXml());

                    returnedObjects.AddRange(new IshObjects(xmlIshObjects).Objects);
                }

                // 3. Write it
                WriteVerbose("returned object count[" + returnedObjects.Count + "]");
                WriteObject(returnedObjects, true);
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
