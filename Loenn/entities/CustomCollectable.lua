local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local CustomCollectable = {}

CustomCollectable.name = "XaphanHelper/CustomCollectable"
CustomCollectable.depth = 0
CustomCollectable.fieldOrder = {
    "x", "y", "sprite", "flag", "collectSound", "newMusic", "mapIcon", "collectGoldenStrawberry", "mustDash", "canRespawn", "changeMusic", "endChapter", "registerInSaveData", "ignoreGolden", "completeSpeedrun"
}
CustomCollectable.placements = {
    name = "CustomCollectable",
    data = {
        sprite = "collectables/XaphanHelper/CustomCollectable/collectable",
        collectSound = "event:/game/07_summit/gem_get",
        changeMusic = false,
        newMusic = "",
        flag = "",
        mapIcon = "",
        mustDash = false,
        collectGoldenStrawberry = false,
        endChapter = false,
        completeSpeedrun = false,
        registerInSaveData = false,
        ignoreGolden = false,
        canRespawn = false
    }
}

function CustomCollectable.sprite(room, entity)
    local texture = entity.sprite or "collectables/XaphanHelper/CustomCollectable/collectable"
    local sprite = drawableSprite.fromTexture(texture .. "00", entity)
    sprite:addPosition(8, 8)

    return sprite
end

function CustomCollectable.selection(room, entity)
    return utils.rectangle(entity.x, entity.y , 16, 16)
end

return CustomCollectable