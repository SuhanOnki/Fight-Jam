using System.IO;
using UnityEditor;
using UnityEngine;

namespace YsoCorp {
    namespace GameUtils {

        public class YCGameutilsUpdateWindow : YCCustomWindow {

            private static string GU_PATH = "Assets/GameUtils";

            private YCUpdatesHandler.GUUpdateData data;
            private bool deleteFolder;
            private bool deleteOthers;

            public void Init(YCUpdatesHandler.GUUpdateData updateData) {
                this.data = updateData;

                this.deleteFolder = true;
                this.deleteOthers = true;
                this.SetSize(450, 150);
                this.SetPosition(WindowPosition.MiddleCenter);
            }

            private void OnGUI() {
                string ok = "";
                GUIStyle style = new GUIStyle(GUI.skin.label);
                if (this.data.isUpToDate) {
                    style.normal.textColor = Color.green;
                    this.AddLabel("GameUtils is already up to date.", TextAnchor.MiddleCenter, style);
                    ok = "Reimport";
                } else {
                    style.normal.textColor = Color.red;
                    this.AddLabel("GameUtils needs to be updated.", TextAnchor.MiddleCenter, style);
                    ok = "Update";
                }
                this.AddEmptyLine();
                this.AddLabel("Do you want to import the version " + this.data.version + "?");
                this.deleteFolder = this.AddToggle("Delete the Gameutils folder before importing (recommended)", this.deleteFolder);
                this.deleteOthers = this.AddToggle("Delete other recommended folders before importing (recommended)", this.deleteOthers);
                this.AddEmptyLine();
                this.AddCancelOk("Cancel", ok, () => this.Close(), () => {
                    if (this.deleteFolder || this.deleteOthers) {
                        if (this.deleteFolder) {
                            YCIOHandler.DeleteDirectory(GU_PATH, true);
                        }
                        if (this.deleteOthers) {
                            foreach (string path in this.data.additionalRemovalsPaths) {
                                if (YCIOHandler.IsDirectory(path)) {
                                    YCIOHandler.DeleteDirectory(path, true);
                                } else {
                                    YCIOHandler.DeleteFile(path);
                                }
                            }
                        }
                        AssetDatabase.Refresh();
                    }
                    YCUpdatesHandler.ImportGameutilsPackage(this.data);
                    this.Close();
                });
            }
        }
    }
}
