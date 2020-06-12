namespace Adjustment
{
    class PublicClass               //定义此公共类，用来主窗体和CMD的之间传递命令。否则cmd命令在主窗体不能执行。
    {
        private static AuroraMain formAuroraMain;
        public static AuroraMain AuroraMain
        {
            get { return formAuroraMain; }
        }

        private static MyCmd formMyCmd;
        public static MyCmd MyCmd
        {
            get { return formMyCmd; }
        }

        private static Locker formLocker;
        public static Locker Locker
        {
            get { return formLocker; }
        }

        private static TimeLevel formTimeLevel;
        public static TimeLevel TimeLevel
        {
            get { return formTimeLevel; }
        }

        static PublicClass()
        {
            formAuroraMain = new AuroraMain();
            formMyCmd = new MyCmd();
            formLocker = new Locker();
            formTimeLevel = new TimeLevel();
        }
    }
}
