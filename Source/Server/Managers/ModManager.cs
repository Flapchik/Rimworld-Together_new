using Newtonsoft.Json;
using RimworldTogether.GameServer.Core;
using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.JSON;
using Shared.Misc;
using Shared.Network;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace RimworldTogether.GameServer.Managers
{
    public static class ModManager
    {
        public static List<ModFile> requiredMods = new List<ModFile>();
        public static List<ModFile> optionalMods = new List<ModFile>();

        public enum ModType { Required, Optional }

        public static void LoadMods()
        {
            Logger.WriteToConsole($"Checking for server mods.", Logger.LogMode.Warning);

            ZipModFolder(ModType.Required);
            ZipModFolder(ModType.Optional);
        }

        private static void ZipModFolder(ModType modType)
        {
            string pathToUse = "";
            if (modType == ModType.Required) pathToUse = Program.requiredModsPath;
            else if (modType == ModType.Optional) pathToUse = Program.optionalModsPath;

            if (GetModCountInFolder(pathToUse) == 0) return;
            else
            {
                string zipPath = Path.Combine(pathToUse + ".zip");

                if (File.Exists(zipPath))
                {
                    Logger.WriteToConsole($"Zip file at '{zipPath}' already existed, ignoring.", Logger.LogMode.Warning);
                }

                else
                {
                    ZipFile.CreateFromDirectory(pathToUse, zipPath);
                    Logger.WriteToConsole($"Prepared mod zip file at '{zipPath}'", Logger.LogMode.Warning);
                }
            }
        }

        public static void SendModListPartToClient(ServerClient client)
        {
            if (client.uploadManager == null)
            {
                Logger.WriteToConsole($"[Send mods] > {client.username} | {client.SavedIP}");

                client.uploadManager = new UploadManager();
                client.uploadManager.PrepareUpload(Program.modsPath + Path.DirectorySeparatorChar + "Required.zip");
            }

            FileTransferJSON fileTransferJSON = new FileTransferJSON();
            fileTransferJSON.fileSize = client.uploadManager.fileSize;
            fileTransferJSON.fileParts = client.uploadManager.fileParts;
            fileTransferJSON.fileBytes = client.uploadManager.ReadFilePart();
            fileTransferJSON.isLastPart = client.uploadManager.isLastPart;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.ManageModlistPacket), fileTransferJSON);
            client.clientListener.SendData(packet);

            if (client.uploadManager.isLastPart) client.uploadManager = null;
        }

        private static int GetModCountInFolder(string pathToCheck)
        {
            return Directory.GetDirectories(pathToCheck).Count();
        }

        public static bool CheckIfModConflict(ServerClient client, JoinDetailsJSON loginDetails)
        {
            //SendModListPartToClient(client);

            //TODO
            //Create 2 FileTransferJSONs to send both the required mods and the optional ones, if any
            //Also check if the client already had the mods needed

            return false;
        }
    }
}
