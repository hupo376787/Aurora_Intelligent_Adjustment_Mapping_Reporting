
namespace Adjustment
{
    class Constants
    {
        public const string CopyRight = "Welcome to Aurora Command Line [Version 1.0]" + "\r\n"
                                        + "CopyRight: Aurora 2013. Developed by hupo376787.\r\n";

        public const string LabelFormat = "\r\nInput Command>";

        public const string Label = "Input Command>";

        public const string ErrorFormat = "\r\n\"{0}\" is unknown command. Input '-help' for more information.";

        public const string CmdAddRow = "addrow";               //全部转换为小写

        public const string CmdDeleteRow = "deleterow";

        public const string CmdClearList = "clearlist";

        public const string CmdCalcAdj = "calcadj";

        public const string CmdDrawMap = "drawmap";

        public const string CmdReport = "report";

        //下面是Windows命令行指令
        public const string CmdNotepad = "notepad";
        public const string CmdCalc = "calc";
        public const string CmdMspaint = "mspaint";
        public const string CmdCmd = "cmd";
        public const string CmdExplorer = "explorer";
        public const string CmdRegedit = "regedit";
        public const string CmdTaskmgr = "taskmgr";
        public const string CmdWrite = "write";

        public const string Cmdhupo376787 = "hupo376787";

        public const string CmdNokia = "nokia";

        public const string CmdHelp = "-help";

        public const string Help = "\r\n欢迎使用Aurora 命令行智能帮助系统。目前支持Aurora内部常见的命令和Windows的部分命令行指令(不区分大小写)。" + "\r\n"
                                    + "AddRow：增加一行数据。" + "\r\n"
                                    + "DeleteRow：删除选中行数据。" + "\r\n"
                                    + "ClearList：清空所有数据。" + "\r\n"
                                    + "CalcAdj：开始平差计算。" + "\r\n"
                                    + "DrawMap：绘制点位图。" + "\r\n"
                                    + "Report：生成报表。" + "\r\n";
       
    }
}
