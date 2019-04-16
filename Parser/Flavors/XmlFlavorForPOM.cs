using System;
using System.Collections.Generic;
using System.Linq;
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
                                                                            ElementNames.CiManagement,
                                                                            ElementNames.DistributionManagement,
                                                                            ElementNames.DependencyManagement,
                                                                            ElementNames.Dependency,
                                                                            ElementNames.Repository,
                                                                            ElementNames.PluginRepository,
                                                                            ElementNames.Plugin,
                                                                            ElementNames.Reporting,
                                                                            ElementNames.Connection,
                                                                            ElementNames.DeveloperConnection,
                                                                            ElementNames.ArtifactsToPublish,
                                                                            ElementNames.ArtifactsToDownload,
                                                                            ElementNames.Tag,
                                                                            ElementNames.GitRepositoryName,
                                                                            ElementNames.System,
                                                                            ElementNames.Id,
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, ElementNames.Project, StringComparison.OrdinalIgnoreCase)
                                                         && string.Equals(info.Namespace, "http://maven.apache.org/POM/4.0.0", StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.LocalName : base.GetName(reader);

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node is Container c)
            {
                var nodeType = node.Type;
                switch (nodeType)
                {
                    case ElementNames.ArtifactId:
                    case ElementNames.GroupId:
                    case ElementNames.Module:
                    case ElementNames.Name:
                    case ElementNames.Id:
                    case ElementNames.Url:
                    case ElementNames.Connection:
                    case ElementNames.DeveloperConnection:
                    case ElementNames.Tag:
                    case ElementNames.GitRepositoryName:
                    case ElementNames.System:
                        node.Name = c.Children.FirstOrDefault(_ => _.Type == NodeType.Text)?.Content;
                        break;

                    default:
                        var type = GetNamingElementType(nodeType);
                        node.Name = c.Children.FirstOrDefault(_ => _.Type == type)?.Name;
                        break;
                }
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);

        private static string GetNamingElementType(string nodeType)
        {
            switch (nodeType)
            {
                case ElementNames.Developer:
                case ElementNames.License:
                case ElementNames.Organization:
                    return ElementNames.Name;

                case ElementNames.Profile:
                    return ElementNames.Id;

                case ElementNames.IssueManagement:
                    return ElementNames.System;
                default:
                    return ElementNames.GroupId;
            }
        }

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
            internal const string Plugin = "plugin";
            internal const string Reporting = "reporting";
            internal const string Profile = "profile";
            internal const string Id = "id";
            internal const string Connection = "connection";
            internal const string DeveloperConnection = "developerConnection";
            internal const string ArtifactsToPublish = "artifactsToPublish";
            internal const string ArtifactsToDownload = "artifactsToDownload";
            internal const string Tag = "tag";
            internal const string GitRepositoryName = "gitRepositoryName";
            internal const string System = "system";
        }
    }
}