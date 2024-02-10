using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using RimworldTogether.GameClient.Core;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.JSON;
using Shared.Network;
using Verse;

namespace RimworldTogether.GameClient.Managers
{
    public static class ModManager
    {
        public static string[] GetRunningModList()
        {
            List<string> compactedMods = new List<string>();

            ModContentPack[] runningMods = LoadedModManager.RunningMods.ToArray();
            foreach (ModContentPack mod in runningMods) compactedMods.Add(mod.PackageId);
            return compactedMods.ToArray();
        }

        public static void GetConflictingMods(Packet packet)
        {
            JoinDetailsJSON loginDetailsJSON = (JoinDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            DialogManager.PushNewDialog(new RT_Dialog_Listing("Mod Conflicts", "The following mods are conflicting with the server",
                loginDetailsJSON.conflictingMods.ToArray()));
        }

        public static bool CheckIfMapHasConflictingMods(MapDetailsJSON mapDetailsJSON)
        {
            string[] currentMods = GetRunningModList();

            foreach (string mod in mapDetailsJSON.mapMods)
            {
                if (!currentMods.Contains(mod)) return true;
            }

            foreach (string mod in currentMods)
            {
                if (!mapDetailsJSON.mapMods.Contains(mod)) return true;
            }

            return false;
        }

        public static void ReceiveModsFromServer(Packet packet)
        {
            FileTransferJSON fileTransferJSON = (FileTransferJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            if (Network.Network.serverListener.downloadManager == null)
            {
                Log.Message($"[Rimworld Together] > Receiving mods from server");
                string filePath = Path.Combine(GenFilePaths.ModsFolderPath, "Required.zip");

                Network.Network.serverListener.downloadManager = new DownloadManager();
                Network.Network.serverListener.downloadManager.PrepareDownload(filePath, fileTransferJSON.fileParts);
            }

            Network.Network.serverListener.downloadManager.WriteFilePart(fileTransferJSON.fileBytes);

            if (fileTransferJSON.isLastPart)
            {
                Network.Network.serverListener.downloadManager.FinishFileWrite();
                Network.Network.serverListener.downloadManager = null;

                Log.Message("Finished mod list download");

                //ZipFile.ExtractToDirectory(Path.Combine(GenFilePaths.ModsFolderPath, "Required.zip"), GenFilePaths.ModsFolderPath);
                //File.Delete(Path.Combine(GenFilePaths.ModsFolderPath, "Required.zip"));

                Log.Message("Finished mod list extract");
            }

            else
            {
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(Network.PacketHandler.ManageModlistPacket));
                Network.Network.serverListener.SendData(rPacket);
            }
        }
    }
}
