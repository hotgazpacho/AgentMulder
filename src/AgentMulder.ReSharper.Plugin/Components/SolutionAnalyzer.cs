using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using AgentMulder.ReSharper.Domain.Containers;
using JetBrains.Application;

namespace AgentMulder.ReSharper.Plugin.Components
{
    [ShellComponent]
    public class SolutionAnalyzer
    {
        private readonly PatternSearcher patternSearcher;
        private readonly List<IContainerInfo> knownContainers = new List<IContainerInfo>();

        public void AddContainer(IContainerInfo containerInfo)
        {
            knownContainers.Add(containerInfo);
        }

        public SolutionAnalyzer(PatternSearcher patternSearcher)
        {
            this.patternSearcher = patternSearcher;
            
            LoadContainerInfos();
        }

        private void LoadContainerInfos()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, "Containers");

            if (!Directory.Exists(path))
            {
                return;
            }

            var catalog = new DirectoryCatalog(path, "*.dll");
            var container = new CompositionContainer(catalog);
            IEnumerable<IContainerInfo> values = container.GetExportedValues<IContainerInfo>();
            knownContainers.AddRange(values);
        }

        public IEnumerable<RegistrationInfo> Analyze()
        {
            return knownContainers.SelectMany(ScanRegistrations);
        }

        private IEnumerable<RegistrationInfo> ScanRegistrations(IContainerInfo containerInfo)
        {
            return (from pattern in containerInfo.RegistrationPatterns
                    let matchResults = patternSearcher.Search(pattern)
                    from matchResult in matchResults.Where(result => result.Matched)
                    from registration in pattern.GetComponentRegistrations(matchResult.MatchedElement)
                    select new RegistrationInfo(registration, containerInfo.ContainerDisplayName)).ToList();
        }
    }
}