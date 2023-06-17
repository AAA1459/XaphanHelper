local drawableSprite = require("structs.drawable_sprite")

local BombSwitch = {}

BombSwitch.name = "XaphanHelper/BombSwitch"
BombSwitch.depth = 8999
BombSwitch.fieldOrder = {
    "x", "y", "sprite", "flag", "registerInSaveData"
}
BombSwitch.fieldInformation = {

}
BombSwitch.placements = {
    name = "BombSwitch",
    data = {
        flag = "",
        registerInSaveData = false,
        sprite = "objects/XaphanHelper/BombSwitch"
    }
}

function BombSwitch.sprite(room, entity)
    local texture = entity.sprite or "objects/XaphanHelper/BombSwitch"
    local sprite = drawableSprite.fromTexture(texture .. "/idle00", entity)
    sprite:addPosition(8, 8)

    return sprite
end

return BombSwitch