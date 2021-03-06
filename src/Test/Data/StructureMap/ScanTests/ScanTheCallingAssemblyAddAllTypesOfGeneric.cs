﻿using StructureMap;
using TestApplication.Types;

namespace TestApplication.StructureMap.ScanTests
{
    public class ScanTheCallingAssemblyAddAllTypesOfGeneric
    {
        public ScanTheCallingAssemblyAddAllTypesOfGeneric()
        {
            var container = new Container(x => x.Scan(scanner =>
            {
                scanner.TheCallingAssembly();
                scanner.AddAllTypesOf<ICommon>();
            }));
        } 
    }
}