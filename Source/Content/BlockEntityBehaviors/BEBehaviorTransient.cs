﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.Common;
using Vintagestory.GameContent;

namespace Immersion
{
    public class TransitionConditions
    {
        public TransitionConditions(int RequiredSunlight)
        {
            this.RequiredSunlight = RequiredSunlight;
        }
        public int RequiredSunlight { get; set; } = -1;
    }

    public class BEBehaviorTransient : BlockEntityBehavior
    {
        double transitionAtHour = -1;
        BlockPos Pos { get => Blockentity.Pos; }
        string fromCode;
        string toCode;
        public Block OwnBlock { get => Blockentity.Block; }
        public TransitionConditions conditions;

        public BEBehaviorTransient(BlockEntity blockentity) : base(blockentity)
        {
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            if (transitionAtHour <= 0)
            {
                transitionAtHour = api.World.Calendar.TotalHours + properties["inGameHours"].AsFloat(24);
            }

            fromCode = properties["convertFrom"].AsString()?.WithDomain(OwnBlock.Code.Domain);
            toCode = properties["convertTo"].AsString()?.WithDomain(OwnBlock.Code.Domain);
            conditions = properties["transitionConditions"]?.AsObject<TransitionConditions>(null);

            if (fromCode != null && toCode != null)
            {
                if (api.Side.IsServer())
                {
                    Blockentity.RegisterGameTickListener(CheckTransition, 2000);
                }
            }
            else api.World.BlockAccessor.RemoveBlockEntity(Pos);
        }

        public void CheckTransition(float dt)
        {
            int light = Api.World.BlockAccessor.GetLightLevel(this.Blockentity.Pos, EnumLightLevelType.MaxTimeOfDayLight);

            if (light < (conditions?.RequiredSunlight ?? -1))
            {
                transitionAtHour += (dt / 60);
            }
            Blockentity.MarkDirty();
            
            if (Api.World.Calendar.TotalHours < transitionAtHour) return;

            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            Block tblock;

            if (!toCode.Contains("*"))
            {
                tblock = Api.World.GetBlock(new AssetLocation(toCode));
                if (tblock == null) return;

                Api.World.BlockAccessor.SetBlock(tblock.BlockId, Pos);
                return;
            }

            AssetLocation blockCode = block.WildCardReplace(
                new AssetLocation(fromCode),
                new AssetLocation(toCode)
            );

            tblock = Api.World.GetBlock(blockCode);
            if (tblock == null) return;

            Api.World.BlockAccessor.SetBlock(tblock.BlockId, Pos);
        }

        public override void FromTreeAtributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAtributes(tree, worldForResolving);

            transitionAtHour = tree.GetDouble("transitionAtHour");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetDouble("transitionAtHour", transitionAtHour);
        }

        /*
        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            ICoreClientAPI capi = (Api as ICoreClientAPI);
            GameCalendar cal = (GameCalendar)capi.World.Calendar;
            int fullHour = (int)(transitionAtHour % (double)cal.HoursPerDay);
            string hour = fullHour < 10 ? "0" + fullHour : "" + fullHour;
            int m = (int)(60 * (transitionAtHour - fullHour));
            string minute = m < 10 ? "0" + m : "" + m;
            string time = hour + ":" + minute;


            dsc.AppendLine("Transitions at: " + time + " on ");
            base.GetBlockInfo(forPlayer, dsc);
        }
        */

    }
}
