using ShieldMeBruh.Components;
using UnityEngine;
using UnityEngine.UI;

namespace ShieldMeBruh.Extensions;

public static class ElementExtensions
{

    public static Image GetShield(this InventoryGrid.Element _mElement)
    {
        var shield = ShieldMe.Elements[_mElement.m_pos]?.GetShield();
        return shield;
    }
    
    public static Image CreateShield(this InventoryGrid.Element _mElement)
    {
        Image img;

        ShieldMeBruh.Log.Debug($"Shield Element: {_mElement.m_go.transform.name}");
        img = _mElement.CreateShieldedImage();
        img.enabled = false;
        
        return img;
    }

    public static Image CreateShieldedImage(this InventoryGrid.Element _mElement)
    {
        var baseImg = _mElement.m_icon;
        var noTeleport = _mElement.m_noteleport;
        
        // set m_queued parent as parent first, so the position is correct
        var obj = Object.Instantiate(baseImg, baseImg.transform.parent);
        // change the parent to the m_queued image so we can access the new image without a loop
        var imgT = obj.transform;
        //transform.SetParent(baseImg.transform);
        imgT.name = "shield";
        //transform.SetAsLastSibling();

        // set the new shield image
        obj.sprite = ShieldMeBruh.AutoShield.Shield;
        obj.name = "shield";
        obj.color = noTeleport.color;
        obj.type = noTeleport.type;
        
        return obj;
    }
    

}