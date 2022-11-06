using Verse;

namespace Clonebay
{
    public class Dialog_RenameCustomNameComp : Dialog_Rename
    {
        public CustomNameComp nameComp;

        protected override void SetName(string name)
        {
            nameComp.customName = name;
        }
    }
}
