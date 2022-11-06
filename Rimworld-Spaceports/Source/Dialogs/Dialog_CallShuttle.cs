using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Spaceports.Dialogs
{
    public class Dialog_CallShuttle : Window
    {
        private Action ConfirmAction;
        private bool EnoughSilver;
        private bool PadAvailable;
        public override Vector2 InitialSize => new Vector2(300f, 190f);
        public Dialog_CallShuttle(Action ConfirmAction, bool EnoughSilver, bool PadAvailable)
        {
            this.ConfirmAction = ConfirmAction;
            this.EnoughSilver = EnoughSilver;
            this.PadAvailable = PadAvailable;
            forcePause = true;
            closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("Spaceports_Uplink".Translate().Colorize(Color.gray));
            listingStandard.GapLine();
            listingStandard.Label("Spaceports_CallTaxiInfo".Translate());
            if (listingStandard.ButtonText("Spaceports_CallTaxiCancel".Translate()))
            {
                Close();
            }
            if (EnoughSilver == false || PadAvailable == false)
            {
                if (listingStandard.ButtonText("Spaceports_CallTaxiConfirm".Translate().Colorize(Color.red)))
                {
                    if (!EnoughSilver)
                    {
                        Messages.Message("Spaceports_CannotProceedSilver".Translate(), MessageTypeDefOf.RejectInput);
                    }
                    if (!PadAvailable)
                    {
                        Messages.Message("Spaceports_CannotProceedNoPads".Translate(), MessageTypeDefOf.RejectInput);

                    }
                }
            }
            else if (EnoughSilver == true && PadAvailable == true)
            {
                if (listingStandard.ButtonText("Spaceports_CallTaxiConfirm".Translate()))
                {
                    Close();
                    SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
                    ConfirmAction();
                }
            }

            listingStandard.End();
        }

    }
}
