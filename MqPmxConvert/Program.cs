using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MqPmxConvert
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            /*
             * 
             * IKボーンの先と同じ位置にターゲットボーンを作成して配置する
             * 重なってていいはず
             * 
             * ik_tip_nameが近づける先のボーン名
             * ボーンとしての登録名がik_name
             * tip_idでIKの先っぽボーンのIDを決める
             * is_id=1は一番ルートから始まる
             * IK_CHAINは回転させるボーンの個数
             */

            string inputPath, outputPath;
            if (args.Length == 0)
            { //入力がないとき
                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.Filter = "対応ファイル|*.mqo;*.pmx";
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                    return;
                inputPath = openFileDialog.FileName;

                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.FileName = openFileDialog.FileName + ".cnv";
                saveFileDialog.Filter = "対応ファイル|*.mqo;*.pmx";
                saveFileDialog.AddExtension = false;
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                    return;
                outputPath = saveFileDialog.FileName;
            }
            else
            { //入力があったとき
                inputPath = args[0];

                if (args.Length >= 2)
                    outputPath = args[1];
                else
                    outputPath = inputPath + ".cnv";
            }

            ProcessFile(
                inputPath,
                outputPath.Substring(0, outputPath.Length - Path.GetExtension(outputPath).Length));

            System.Console.ReadLine();
        }

        public static void ProcessFile(string inputPath, string outputPathWithoutExt)
        {
            switch (System.IO.Path.GetExtension(inputPath))
            {
                case ".mqo":
                    System.Console.WriteLine("MQO形式が選択されました...");
                    AzureCore.Graphic.Azure.Model.Import.MQO.MQOImporter mqo = new AzureCore.Graphic.Azure.Model.Import.MQO.MQOImporter();
                    mqo.ConvertMqo(inputPath, outputPathWithoutExt);
                    break;
                
                case ".pmx":
                    System.Console.WriteLine("PMX形式が選択されました...");
                    AzureCore.Graphic.Azure.Model.Import.MQO.PMXImporter pmxi = new AzureCore.Graphic.Azure.Model.Import.MQO.PMXImporter();
                    pmxi.LoadPmx(inputPath, outputPathWithoutExt);
                    break;
            }

            System.Console.WriteLine("");
            System.Console.WriteLine("正常終了");
        }
    }
}
