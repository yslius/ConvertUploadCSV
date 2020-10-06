using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace ConvertUploadCSV
{
    public partial class Form1 : Form
    {
        convFile cconvFile;
        public Form1()
        {
            InitializeComponent();
            cconvFile = new convFile();
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            // GetDataの戻り値はstring型の配列であり、
            // 複数のファイルがドロップされた場合には
            // ドロップされた複数のファイル名が取得できる。

            for (int i = 0; i < files.Length; i++)
            {
                // GetDataにより取得したString型の配列から要素を取り出す。
                var pathFile = files[i];
                string fileName = Path.GetFileName(pathFile);
                Debug.WriteLine(pathFile);
                Debug.WriteLine(fileName);

                DialogResult res = 
                    MessageBox.Show("このファイルを変換しますか？\n" + fileName,
                    "ConvertUploadCSV",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1);
                if (res == DialogResult.Yes)
                {
                    if(cconvFile.ConvCSV(pathFile) < 0)
                        Environment.Exit(0);
                }
                else if (res == DialogResult.No)
                {
                    continue;
                }
                else if (res == DialogResult.Cancel)
                {
                    // 終了
                    Environment.Exit(0);
                }
                break;
            }

            DialogResult resOK =
                    MessageBox.Show("正常に変換しました。",
                    "ConvertUploadCSV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            Environment.Exit(0);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            // マウスポインター形状変更
            //
            // DragDropEffects
            //  Copy  :データがドロップ先にコピーされようとしている状態
            //  Move  :データがドロップ先に移動されようとしている状態
            //  Scroll:データによってドロップ先でスクロールが開始されようとしている状態、あるいは現在スクロール中である状態
            //  All   :上の3つを組み合わせたもの
            //  Link  :データのリンクがドロップ先に作成されようとしている状態
            //  None  :いかなるデータもドロップ先が受け付けようとしない状態

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
    }
}
