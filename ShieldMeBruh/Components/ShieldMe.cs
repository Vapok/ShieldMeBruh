using System.Collections.Generic;
using ShieldMeBruh.Extensions;
using ShieldMeBruh.Features;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;

namespace ShieldMeBruh.Components;

public class ShieldMe : MonoBehaviour
{
    public static readonly Dictionary<Vector2i,ShieldMe> Elements = [];
    private InventoryGrid.Element _mElement;
    private InventoryGrid _mGrid;
    private UIInputHandler _mInputHandler;
    private ItemDrop.ItemData _mItem;
    private Image _shieldImage;
    private bool _active;
    private GameObject _shieldGo;
    
    private void Awake()
    {
        ShieldMeBruh.Log.Debug("ShieldMe Element Awake");
    }

    private void Start()
    {
        ShieldMeBruh.Log.Debug("ShieldMe Element Started");
        _shieldImage = _mElement.CreateShield();
        _shieldGo = _shieldImage.gameObject;
        ShieldMeBruh.Log.Debug($"Loaded GameObject: {_shieldGo.name}  Transform: {_shieldGo.transform.name}");
        var savedElementVector = ShieldMeBruh.AutoShield.GetShieldSaveData().SavedElement;

        if (_mElement.m_pos != savedElementVector) return;
        
        _mItem = _mGrid.m_inventory.GetItemAt(_mElement.m_pos.x, _mElement.m_pos.y);
        
        ShieldMeBruh.Log.Debug($"_mItem is null: {_mItem == null}");
        ApplyShieldToElement();
        InvokeRepeating(nameof(UpdateShield),2f,2f);
    }

    private void UpdateShield()
    {
        if (!_active || _shieldImage.enabled) return;
        
        _mItem = _mGrid.m_inventory.GetItemAt(_mElement.m_pos.x, _mElement.m_pos.y);
        
        RefreshShield();
    }
    
    public static ShieldMe GetShieldFromElement(Vector2i pos)
    {
        return Elements[pos];
    }
    
    public void SetElement(InventoryGrid.Element element)
    {
        _mElement = element;
        
        if (!Elements.ContainsKey(element.m_pos))
            Elements.Add(element.m_pos,this);
        
        _mInputHandler = gameObject.GetComponentInChildren<UIInputHandler>();
        _mInputHandler.m_onMiddleDown += OnMiddleClick;
        ShieldMeBruh.Log.Debug($"Adding to element: X: {_mElement.m_pos.x}  Y: {_mElement.m_pos.y}");
    }
    

    public void SetGrid(InventoryGrid inventoryGrid)
    {
        _mGrid = inventoryGrid;
    }

    public Image GetShield()
    {
        if (!_shieldImage)
            _shieldImage = _shieldGo.GetComponent<Image>();
        
        return _shieldImage;
    }
    
    public void ApplyShieldToElement(ItemDrop.ItemData itemOverride = null, bool allowReset = false)
    {
        ShieldMeBruh.Log.Debug($"ApplyShieldToElement called on Grid Element {_mElement.m_pos.x},{_mElement.m_pos.y}");
        
        ShieldMeBruh.Log.Debug($"_mItem is null: {_mItem == null}");

        _mItem ??= _mGrid.m_inventory.GetItemAt(_mElement.m_pos.x, _mElement.m_pos.y);
        
        var selectedItem = itemOverride != null ? itemOverride : _mItem;
        
        ShieldMeBruh.Log.Debug($"selectedItem is null: {selectedItem == null}");
        ShieldMeBruh.Log.Debug($"_shieldImage is null: {!_shieldImage}");
        ShieldMeBruh.Log.Debug($"SelectedItem: {selectedItem?.m_shared.m_name}");
        ShieldMeBruh.Log.Debug($"_mItem is null: {_mItem == null}");
        
        if (selectedItem?.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield)
            return;

        _shieldImage.enabled = true;

        if (ShieldMeBruh.AutoShield.CurrentElement == null)
        {
            ShieldMeBruh.AutoShield.CurrentElement = _mElement;
            ShieldMeBruh.AutoShield.SelectedShield = selectedItem;
        }
        else
        {
            if ((ShieldMeBruh.AutoShield.CurrentElement.m_pos == _mElement.m_pos && allowReset) || _mElement.m_pos.x < 0 ||
                _mElement.m_pos.y < 0)
            {
                _shieldImage.enabled = false;
                ShieldMeBruh.AutoShield.CurrentElement = null;
                ShieldMeBruh.AutoShield.SelectedShield = null;
            }
            else
            {
                ShieldMeBruh.AutoShield.CurrentElement = _mElement;
                ShieldMeBruh.AutoShield.SelectedShield = selectedItem;
            }
        }

        var saveVector = new Vector2i(-1, -1);
        
        if (ShieldMeBruh.AutoShield.CurrentElement != null)
            saveVector = new Vector2i(_mElement.m_pos.x, _mElement.m_pos.y);

        var savedData = new AutoShieldSaveData();
        savedData.SavedElement = saveVector;

        SaveShieldSaveData(savedData);
        _active = true;
        RefreshShield();
        
        ShieldMeBruh.Log.Debug($"ApplyShieldToElement fished on Grid Element {_mElement.m_pos.x},{_mElement.m_pos.y}");
        ShieldMeBruh.Log.Debug($"CurrentElement: [{ShieldMeBruh.AutoShield.CurrentElement?.m_pos.x},{ShieldMeBruh.AutoShield.CurrentElement?.m_pos.y}]");
        ShieldMeBruh.Log.Debug($"SelectedItem: [{ShieldMeBruh.AutoShield.SelectedShield?.m_shared.m_name}]");
    }
    
    private void SaveShieldSaveData(AutoShieldSaveData savedData)
    {
        var serializer = new SerializerBuilder().Build();

        var yaml = serializer.Serialize(savedData);

        Player.m_localPlayer.m_customData[ShieldMeBruh.m_instance.PluginId] = yaml;

    }
    
    private void OnMiddleClick(UIInputHandler middleClick)
    {
        if (!ShieldMeBruh.AutoShield.FeatureInitialized || Player.m_localPlayer == null || _mGrid == null)
            return;

        if (middleClick == null || middleClick.gameObject == null) return;

        var player = Player.m_localPlayer;
        
        var buttonPos = _mElement.m_pos; //_activeInstance.GetButtonPos(middleClick.gameObject);
        ShieldMeBruh.Log.Debug($"Button Pressed on {buttonPos.x},{buttonPos.y}");

        _mItem = _mGrid.m_inventory.GetItemAt(buttonPos.x, buttonPos.y);

        if (_mItem == null) return;

        ShieldMeBruh.Log.Debug($"Item Name {_mItem.m_shared.m_name} of type {_mItem.m_shared.m_itemType}");


        if (_mItem.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield) return;

        var targetVector = new Vector2i(buttonPos.x, buttonPos.y);

        if (ShieldMeBruh.AutoShield.CurrentElement == null)
        {
            ApplyShieldToElement(allowReset:true);
        }
        else if (ShieldMeBruh.AutoShield.CurrentElement.m_pos == targetVector)
        {
            ResetCurrentSheildElement();
        }
        else if (ShieldMeBruh.AutoShield.CurrentElement.m_pos != targetVector)
        {
            var oldShieldItem = _mGrid.GetInventory().GetItemAt(ShieldMeBruh.AutoShield.CurrentElement.m_pos.x, ShieldMeBruh.AutoShield.CurrentElement.m_pos.y);
            var newShieldItem = _mItem;

            var oldShield = Elements[ShieldMeBruh.AutoShield.CurrentElement.m_pos];
            oldShield.ResetCurrentSheildElement();
            ApplyShieldToElement(allowReset:true);

            if (oldShieldItem != null && oldShieldItem.m_equipped) player.EquipItem(newShieldItem);
        }
    }
    
    public void ResetCurrentSheildElement()
    {
        ShieldMeBruh.Log.Debug($"ResetCurrentShieldElement called on Grid Element {_mElement.m_pos.x},{_mElement.m_pos.y}");
        ShieldMeBruh.Log.Debug($"CurrentElement Grid Element {ShieldMeBruh.AutoShield.CurrentElement?.m_pos.x},{ShieldMeBruh.AutoShield.CurrentElement?.m_pos.y}");
        ShieldMeBruh.Log.Debug($"Current Shield Status: {GetShield().IsActive()}");
        ShieldMeBruh.Log.Debug($"Start SelectedItem: [{ShieldMeBruh.AutoShield.SelectedShield?.m_shared.m_name}]");
        ShieldMeBruh.Log.Debug($"Active: [{_active}]");
        
        if (!_active) return;
        
        _shieldImage.enabled = false;

        ShieldMeBruh.AutoShield.CurrentElement = null;
        ShieldMeBruh.AutoShield.SelectedShield = null;

        var savedData = new AutoShieldSaveData();
        savedData.SavedElement = new Vector2i(-1, -1);

        SaveShieldSaveData(savedData);
        _active = false;
        
        ShieldMeBruh.Log.Debug($"ResetCurrentShieldElement Finished Grid Element {_mElement.m_pos.x},{_mElement.m_pos.y}");
        ShieldMeBruh.Log.Debug($"CurrentElement Grid Element {ShieldMeBruh.AutoShield.CurrentElement?.m_pos.x},{ShieldMeBruh.AutoShield.CurrentElement?.m_pos.y}");
        ShieldMeBruh.Log.Debug($"Current Shield Status: {GetShield().IsActive()}");
        ShieldMeBruh.Log.Debug($"End SelectedItem: [{ShieldMeBruh.AutoShield.SelectedShield?.m_shared.m_name}]");
    }
    
    public void RefreshShield()
    {
        var currentElement = ShieldMeBruh.AutoShield.CurrentElement;
        ShieldMeBruh.Log.Debug($"RefreshShield: currentElement is null: {currentElement == null}");
        ShieldMeBruh.Log.Debug($"RefreshShield: _mElement is null: {_mElement == null}");
        ShieldMeBruh.Log.Debug($"RefreshShield: _shieldImage is null: {_shieldImage == null}");
        ShieldMeBruh.Log.Debug($"RefreshShield: _active: {_active}");
        ShieldMeBruh.Log.Debug($"RefreshShield: _mItem == null: {_mItem == null}");
        
        var shield = GetShield();

        if (!_active || _mElement == null)
        {
            shield.enabled = false;
            return;
        }

        _mItem = _mGrid.m_inventory.GetItemAt(_mElement.m_pos.x, _mElement.m_pos.y);

        if (_mItem == null)
        {
            shield.enabled = false;
            return;
        }

        if (ShieldMeBruh.AutoShield.FeatureInitialized)
        {
            if (_mElement?.m_pos == currentElement?.m_pos)
                shield.enabled = true;
            else
                shield.enabled = false;
            
            ShieldMeBruh.Log.Debug($"RefreshShield: Shield Enabled1: {shield.enabled}");
            return;
        }

        if (currentElement != null)
            shield.enabled = false;
        
        ShieldMeBruh.Log.Debug($"RefreshShield: Shield Enabled2: {shield.enabled}");
    } 

}