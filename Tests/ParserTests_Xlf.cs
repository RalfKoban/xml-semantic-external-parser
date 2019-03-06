using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests_Xlf
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "xliff1.xlf");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches()
        {
            Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "xliff1.xlf"));
        }

        [Test]
        public void Bla()
        {
            var expectedNames = new[]
                                    {
                                        "AppVeyorSettingsUserControl",
                                        "AutoCompileSubModulesPlugin",
                                        "BackgroundFetchPlugin",
                                        "BitbucketPlugin",
                                        "BitbucketPullRequestForm",
                                        "CreateLocalBranchesForm",
                                        "CreateLocalBranchesPlugin",
                                        "DeleteUnusedBranchesForm",
                                        "DeleteUnusedBranchesPlugin",
                                        "FindLargeFilesForm",
                                        "FindLargeFilesPlugin",
                                        "FormGerritChangeSubmitted",
                                        "FormGerritDownload",
                                        "FormGerritPublish",
                                        "FormGitReview",
                                        "FormGitStatistics",
                                        "FormImpact",
                                        "FormPluginInformation",
                                        "GerritPlugin",
                                        "GerritSettings",
                                        "GitFlowForm",
                                        "GitFlowPlugin",
                                        "GitHub3Plugin",
                                        "GitImpactPlugin",
                                        "GitStatisticsPlugin",
                                        "GourcePlugin",
                                        "GourceStart",
                                        "JenkinsSettingsUserControl",
                                        "JiraCommitHintPlugin",
                                        "ProxySwitcherForm",
                                        "ProxySwitcherPlugin",
                                        "ReleaseNotesGeneratorForm",
                                        "ReleaseNotesGeneratorPlugin",
                                        "TeamCitySettingsUserControl",
                                        "TfsSettingsUserControl",
                                        "VstsAndTfsSettingsUserControl",
                                    };

            Assert.That(_root.Children.Count, Is.EqualTo(36));

            var names = _root.Children.Select(_ => _.Name).ToList();
            Assert.That(names, Is.EqualTo(expectedNames));
        }
    }
}