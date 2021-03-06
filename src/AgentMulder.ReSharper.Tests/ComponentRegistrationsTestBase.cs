using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgentMulder.ReSharper.Domain.Containers;
using AgentMulder.ReSharper.Plugin.Components;
using JetBrains.Application.Components;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.TestFramework;
using JetBrains.Util;
using NUnit.Framework;

namespace AgentMulder.ReSharper.Tests
{
    [TestFixture]
    public abstract class ComponentRegistrationsTestBase : BaseTestWithSingleProject
    {
        protected abstract IContainerInfo ContainerInfo { get; }

        protected virtual string RelativeTypesPath
        {
            get { return "..\\Types"; }
        }

        protected void RunTest(string testName, Action<List<RegistrationInfo>> action)
        {
            string fileName = testName + Extension;
            var dataPath = new DirectoryInfo(Path.Combine(SolutionItemsBasePath, RelativeTypesPath));
            var fileSet = dataPath.GetFiles("*.cs").SelectNotNull(fileInfo => Path.Combine(RelativeTypesPath, fileInfo.Name)).Concat(new[] { fileName });

            WithSingleProject(fileSet, (lifetime, project) => RunGuarded(() =>
            {
                var searchDomainFactory = ShellInstance.GetComponent<SearchDomainFactory>();
                var patternSearcher = new PatternSearcher(searchDomainFactory);
                var solutionAnalyzer = new SolutionAnalyzer(patternSearcher);
                solutionAnalyzer.AddContainer(ContainerInfo);

                var componentRegistrations = solutionAnalyzer.Analyze();

                action(componentRegistrations.ToList());
            }));
        }

        protected ICSharpFile GetCodeFile(string fileName)
        {
            PsiManager manager = PsiManager.GetInstance(Solution);
            IProjectFile projectFile = Project.GetAllProjectFiles(file => file.Name.Equals(fileName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (projectFile == null)
                return null;

            IPsiSourceFile psiSourceFile = projectFile.ToSourceFile();
            if (psiSourceFile == null)
                return null;

#if SDK70
            var cSharpFile = manager.GetPsiFile(psiSourceFile, CSharpLanguage.Instance, 
                new DocumentRange(psiSourceFile.Document, psiSourceFile.Document.DocumentRange)) as ICSharpFile;
#else
            var cSharpFile = manager.GetPsiFile(psiSourceFile, CSharpLanguage.Instance) as ICSharpFile;
#endif
            if (cSharpFile == null || string.IsNullOrEmpty(cSharpFile.GetText()))
            {
                Assert.Fail("Unable to open the file '{0}', or the file is empty", fileName);
            }

            return cSharpFile;
        }
    }
}