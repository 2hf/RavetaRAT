using Server.RenamingObfuscation.Interfaces;
using dnlib.DotNet;

namespace Server.RenamingObfuscation.Classes
{
    public class PropertiesRenaming : IRenaming
    {
        public ModuleDefMD Rename(ModuleDefMD module)
        {
            ModuleDefMD moduleToRename = module;

            foreach (TypeDef type in moduleToRename.GetTypes())
            {
                if (type.IsGlobalModuleType || type.Name.Contains("Object") || type.Name.Contains("Data") || type.Name.Contains("Packet") || type.Name.Contains("Client"))
                    continue;

                foreach (var property in type.Properties)
                {
                    property.Name = Utils.GenerateRandomString();
                }
            }

            return moduleToRename;
        }
    }
}
