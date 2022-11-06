using SharpUtils;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Spaceports.Buildings
{
    class Building_ShuttleSpot : Building
    {
        private int AccessState = 0;
        private SharpAnim.DrawOverMulti AccessOverlay;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            AccessOverlay = new SharpAnim.DrawOverMulti(SpaceportsMats.ChillSpot, this, 1f, 1f);
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref AccessState, "accessState", 0);
        }

        public override void Draw()
        {
            base.Draw();
            if (AccessState == -1)
            {
                AccessOverlay.SetFrame(0);
            }
            if (AccessState == 0)
            {
                AccessOverlay.SetFrame(1);
            }
            if (AccessState == 1)
            {
                AccessOverlay.SetFrame(2);
            }
            if (AccessState == 2)
            {
                AccessOverlay.SetFrame(3);
            }
            AccessOverlay.Draw();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Command_Action()
            {

                defaultLabel = "AccessControlButton".Translate(),
                defaultDesc = "AccessControlDesc".Translate(),
                icon = GetAccessIcon(),
                Order = -100,
                action = delegate ()
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();

                    foreach (Utils.AccessControlState state in SpaceportsMisc.AccessStates)
                    {
                        if (state.getValue() != 3)
                        {
                            string label = state.GetLabel();
                            FloatMenuOption option = new FloatMenuOption(label, delegate ()
                            {
                                SetAccessState(state.getValue());
                            });
                            options.Add(option);
                        }
                    }

                    if (options.Count > 0)
                    {
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                }
            };

        }

        private void SetAccessState(int val)
        {
            AccessState = val;
        }

        public bool CheckAccessGranted(int val)
        {
            if (AccessState == -1)
            {
                return false;
            }
            if (AccessState == 0 || val == 0)
            {
                return true;
            }
            else return AccessState == val;
        }

        private Texture2D GetAccessIcon()
        {
            if (AccessState == -1)
            {
                return ContentFinder<Texture2D>.Get("Buildings/SpaceportChillSpot/ChillSpotOverlay/ChillSpot_a", true);
            }
            else if (AccessState == 0)
            {
                return ContentFinder<Texture2D>.Get("Buildings/SpaceportChillSpot/ChillSpotOverlay/ChillSpot_b", true);
            }
            else if (AccessState == 1)
            {
                return ContentFinder<Texture2D>.Get("Buildings/SpaceportChillSpot/ChillSpotOverlay/ChillSpot_c", true);
            }
            else if (AccessState == 2)
            {
                return ContentFinder<Texture2D>.Get("Buildings/SpaceportChillSpot/ChillSpotOverlay/ChillSpot_d", true);
            }
            return null;
        }
    }
}
