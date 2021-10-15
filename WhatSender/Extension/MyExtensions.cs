using MetroFramework;
using MetroFramework.Components;
using MetroFramework.Forms;
using System.ComponentModel;
using System.Threading;

namespace WhatSender
{
    public static class MyExtensions
    {
        //What is your style
        private const MetroColorStyle FormStyle = MetroColorStyle.Green;
        private const MetroThemeStyle FormTheme = MetroThemeStyle.Default;
        public static void SetStyle(this IContainer container, MetroForm ownerForm)
        {
            if (container == null)
            {
                container = new System.ComponentModel.Container();
            }
            var manager = new MetroFramework.Components.MetroStyleManager(container);
            manager.Owner = ownerForm;
            container.SetDefaultStyle(ownerForm, FormStyle);
        }
        public static void SetTheme(this IContainer container, MetroForm ownerForm)
        {
            if (container == null)
            {
                container = new System.ComponentModel.Container();
            }
            var manager = new MetroFramework.Components.MetroStyleManager(container);
            manager.Owner = ownerForm;
            container.SetDefaultTheme(ownerForm, FormTheme);
        }
        public static void SetDefaultStyle(this IContainer contr, MetroForm owner, MetroColorStyle style)
        {
            MetroStyleManager manager = FindManager(contr, owner);
            manager.Style = style;
            owner.Style = style;
        }
        public static void SetDefaultTheme(this IContainer contr, MetroForm owner, MetroThemeStyle thme)
        {
            MetroStyleManager manager = FindManager(contr, owner);
            manager.Theme = thme;
        }
        private static MetroStyleManager FindManager(IContainer contr, MetroForm owner)
        {
            MetroStyleManager manager = null;
            foreach (IComponent item in contr.Components)
            {
                if (((MetroStyleManager)item).Owner == owner)
                {
                    manager = (MetroStyleManager)item;
                }
            }
            return manager;
        }
    }
    public class AbortableBackgroundWorker : BackgroundWorker
    {

        private Thread workerThread;

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            workerThread = Thread.CurrentThread;
            try
            {
                base.OnDoWork(e);
            }
            catch (ThreadAbortException)
            {
                e.Cancel = true; //We must set Cancel property to true!
                Thread.ResetAbort(); //Prevents ThreadAbortException propagation
            }
        }


        public void Abort()
        {
            if (workerThread != null)
            {
                workerThread.Abort();
                workerThread = null;
            }
        }
    }
}
