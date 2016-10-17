using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using System.Xml;

namespace Microsoft.SharePoint
{
    public static class SPListExtensions 
    {
        /// <summary>
        /// Add BCS column to SharePoint list
        /// </summary>
        /// <param name="list">List to add the field to</param>
        /// <param name="fieldName">Internal name</param>
        /// <param name="fieldDisplay">Display name</param>
        /// <param name="entityName">Entity as defined in Bdc model, e.g. "Customers"</param>
        /// <param name="bdcFieldName">Field name in related list</param>
        /// <param name="relatedField">Usually entity name + "_ID" (e.g. "Customers_ID")</param>
        /// <param name="systemInstanceName">Model name, e.g. "XYZBdcModel"</param>
        /// <param name="entityNamespace">Model namespace, e.g. "YourCompany.Namespace.Model"</param>
        /// <param name="isRequired">true if column is mandatory</param>
        /// <returns></returns>
        public static bool AddBdcColumn(this SPList list, string fieldName, string fieldDisplay, string entityName, string bdcFieldName, string relatedField, string systemInstanceName, string entityNamespace, bool isRequired) {
            // TODO: replace list Guid
            using (var site = new SPSite(list.ParentWeb.Site.Url))
            {
                using (var web = site.OpenWeb()) {
                    try
                    {
                        web.AllowUnsafeUpdates = true;
                        var list2 = web.Lists[list.ID];
                        if (list2.Fields.ContainsField(fieldName))
                        {
                            list2.Fields.Delete(fieldName);
                            list2.Update();
                        }
                        if (!list2.Fields.ContainsField(fieldName))
                        {
                            // Manually create schemaXML:
                            var schemaXml = string.Format(@"
                                <Field 
                                    Type=""BusinessData"" 
                                    DisplayName=""{0}"" 
                                    Required=""{8}"" 
                                    ID=""{1}"" 
                                    SourceID=""{9}"" 
                                    StaticName=""{2}"" 
                                    BaseRenderingType=""Text"" 
                                    Name=""{2}"" 
                                    ColName=""nvarchar16"" 
                                    RowOrdinal=""0"" 
                                    Version=""2"" 
                                    SystemInstance=""{3}"" 
                                    EntityNamespace=""{4}"" 
                                    EntityName=""{5}"" 
                                    BdcField=""Name"" 
                                    Profile=""/_layouts/15/ActionRedirect.aspx?EntityNamespace={6}&amp;EntityName={1}&amp;LOBSystemInstanceName={3}&amp;ItemID="" 
                                    HasActions=""True"" 
                                    SecondaryFieldBdcNames=""0"" 
                                    RelatedField=""{7}"" 
                                    SecondaryFieldWssNames=""0"" 
                                    RelatedFieldBDCField="""" 
                                    RelatedFieldWssStaticName=""{2}"" 
                                    SecondaryFieldsWssStaticNames=""0"" 
                                    AddFieldOption=""AddToAllContentTypes, AddFieldToDefaultView"" />
                            "
                                , fieldName // 0 - provisionally assign internal name instead of display name
                                , Guid.NewGuid() // 1 - random Guid
                                , fieldName // 2 - internal name
                                , systemInstanceName // 3 
                                , entityNamespace // 4
                                , entityName // 5
                                , entityNamespace.Replace(".", "%2E") // 6 - namespace in url format
                                , relatedField // 7
                                , isRequired.ToString().ToUpper() // 8
                                , string.Format("{{{0}}}", list.ID) // 9
                            );

                            // Add field to list
                            list2.Fields.AddFieldAsXml(schemaXml);
                            list2.Update();

                            // Change field display name
                            var field = (SPBusinessDataField)list2.Fields.GetFieldByInternalName(fieldName);
                            field.Title = fieldDisplay;
                            field.Update();

                            // Add field to view
                            var view = list2.DefaultView;
                            view.ViewFields.Add(field);
                            view.Update();

                            web.AllowUnsafeUpdates = false;

                            // See SPLog class
                            SPLog.WriteLog(SPLog.Category.Information, "AddBdcColumn > Campo agregado: " + fieldName);
                        }
                    }
                    catch (Exception ex)
                    {
                        SPLog.WriteLog(SPLog.Category.Information, "AddBdcColumn > Exception: " + ex.Message);
                        web.AllowUnsafeUpdates = false;
                        return false;
                    }
                    return true;
                }
            }
        }
    }
}