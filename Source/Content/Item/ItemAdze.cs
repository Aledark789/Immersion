﻿using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace Immersion
{
    public class ItemAdze : ItemChisel
	{
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
			if (blockSel == null) return;

            Block block = byEntity.World.BlockAccessor.GetBlock(blockSel.Position);
            if (block.BlockMaterial != EnumBlockMaterial.Wood) return;
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
        }
    }
}