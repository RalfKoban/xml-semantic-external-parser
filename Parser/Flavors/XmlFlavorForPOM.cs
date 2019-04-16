using System;
using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    /// <a href="https://maven.apache.org/xsd/maven-4.0.0.xsd" />
    public sealed class XmlFlavorForPOM : XmlFlavor
    {
        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            ElementNames.ModelVersion,
                                                                            ElementNames.GroupId,
                                                                            ElementNames.ArtifactId,
                                                                            ElementNames.Version,
                                                                            ElementNames.Parent,
                                                                            ElementNames.Packaging,
                                                                            ElementNames.Name,
                                                                            ElementNames.Description,
                                                                            ElementNames.Url,
                                                                            ElementNames.InceptionYear,
                                                                            ElementNames.Organization,
                                                                            ElementNames.License,
                                                                            ElementNames.Developer,
                                                                            ElementNames.Contributor,
                                                                            ElementNames.MailingList,
                                                                            ElementNames.Prerequisites,
                                                                            ElementNames.Module,
                                                                            ElementNames.Scm,
                                                                            ElementNames.IssueManagement,
                                                                            ElementNames.CiManagement,
                                                                            ElementNames.DistributionManagement,
                                                                            ElementNames.DependencyManagement,
                                                                            ElementNames.Dependency,
                                                                            ElementNames.Repository,
                                                                            ElementNames.PluginRepository,
                                                                            ElementNames.Reporting,
                                                                            ElementNames.Profile,
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, ElementNames.Project, StringComparison.OrdinalIgnoreCase)
                                                         && string.Equals(info.Namespace, "http://maven.apache.org/POM/4.0.0", StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.LocalName;
                var attr = GetAttributeName(name);

                return attr is null ? name : reader.GetAttribute(attr);
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);

        private string GetAttributeName(string elementName) => null;

        private static class ElementNames
        {
            internal const string Project = "project";
            internal const string ModelVersion = "modelVersion";
            internal const string Parent = "parent";
            internal const string GroupId = "groupId";
            internal const string ArtifactId = "artifactId";
            internal const string Version = "version";
            internal const string Packaging = "packaging";
            internal const string Name = "name";
            internal const string Description = "description";
            internal const string Url = "url";
            internal const string InceptionYear = "inceptionYear";
            internal const string Organization = "organization";
            internal const string License = "license";
            internal const string Developer = "developer";
            internal const string Contributor = "contributor";
            internal const string MailingList = "mailingList";
            internal const string Prerequisites = "prerequisites";
            internal const string Module = "module";
            internal const string Scm = "scm";
            internal const string IssueManagement = "issueManagement";
            internal const string CiManagement = "ciManagement";
            internal const string DistributionManagement = "distributionManagement";
            internal const string DependencyManagement = "dependencyManagement";
            internal const string Dependency = "dependency";
            internal const string Repository = "repository";
            internal const string PluginRepository = "pluginRepository";
            internal const string Reporting = "reporting";
            internal const string Profile = "profile";
        }

        private static class AttributeNames
        {
        }
    }
}