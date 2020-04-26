using KdTree;
using KdTree.Math;
using PEPlugin;
using PEPlugin.SDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NormalSmoother
{
    public class NormalSmoother : PEPluginClass
    {
        public NormalSmoother() : base()
        {
        }

        public override string Name
        {
            get
            {
                return "選択頂点の同位置法線平均化";
            }
        }

        public override string Version
        {
            get
            {
                return "0.0";
            }
        }

        public override string Description
        {
            get
            {
                return "選択された頂点の内、同位置にある頂点の法線を平均化する";
            }
        }

        public override IPEPluginOption Option
        {
            get
            {
                // boot時実行, プラグインメニューへの登録, メニュー登録名
                return new PEPluginOption(false, true, "選択頂点の同位置法線平均化");
            }
        }

        public override void Run(IPERunArgs args)
        {
            try
            {
                var pmx = args.Host.Connector.Pmx.GetCurrentState();
                var selectedVertexIndices = args.Host.Connector.View.PmxView.GetSelectedVertexIndices();
                // Valueは頂点インデックス
                var tree = new KdTree<float, int>(3, new FloatMath(), AddDuplicateBehavior.List);
                var duplicateVertexIndices = new List<int>();
                foreach (var id in selectedVertexIndices)
                {
                    // 頂点を木に追加
                    tree.Add(pmx.Vertex[id].Position.ToArray(), id);
                }

                // 重複がある頂点の法線を平均化する
                foreach (var node in tree.Where(n => n.Duplicate.Count > 1))
                {
                    var duplicatedPoints = node.Duplicate.Select(i => pmx.Vertex[i]).ToList();
                    var x = duplicatedPoints.Average(v => v.Normal.X);
                    var y = duplicatedPoints.Average(v => v.Normal.Y);
                    var z = duplicatedPoints.Average(v => v.Normal.Z);
                    foreach (var v in duplicatedPoints)
                    {
                        v.Normal = new V3(x, y, z);
                    }
                }

                Utility.Update(args.Host.Connector, pmx, PEPlugin.Pmx.PmxUpdateObject.Vertex);
                MessageBox.Show("完了");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
