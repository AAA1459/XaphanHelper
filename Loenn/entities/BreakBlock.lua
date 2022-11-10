local fakeTilesHelper = require("helpers.fake_tiles")

local BreakBlock = {}

BreakBlock.name = "XaphanHelper/BreakBlock"
BreakBlock.depth = -13000
BreakBlock.fieldOrder = {
    "x", "y", "width", "height", "flag", "mode", "tiletype", "flagTiletype", "directory", "type", "color", "startRevealed", "permanent"
}
function BreakBlock.fieldInformation(entity)
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
        flagTiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
        mode = {
            options = {"Block", "Wall"},
            editable = false
        },
        type = {
            options = {"Bomb", "Drone", "LightningDash", "MegaBomb", "RedBooster", "ScrewAttack"},
            editable = false
        },
        color = {
            fieldType = "color"
        }  
    }
end
BreakBlock.placements = {
    name = "BreakBlock",
    data = {
        width = 8,
        height = 8,
        mode = "Block",
        tiletype = "3",
        flagTiletype = "3",
        flag = "",
        type = "Bomb",
        color = "FFFFFF",
        startRevealed = false,
        directory = "objects/XaphanHelper/BreakBlock",
        permanent = true
    }
}

BreakBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false, "tilesFg", {1.0, 1.0, 1.0, 0.7})

return BreakBlock