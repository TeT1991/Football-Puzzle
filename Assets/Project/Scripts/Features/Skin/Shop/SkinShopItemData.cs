public class SkinShopItemData
{
    private readonly int _id;
    private readonly int _price;
    private readonly bool _isUnlocked;
    private readonly bool _canBuy;

    public SkinShopItemData(int id, int price, bool isUnlocked)
    {
        _id = id;
        _price = price;
        _isUnlocked = isUnlocked;
    }

    public int Id => _id;
    public int Price => _price;
    public bool IsUnlocked => _isUnlocked;
}
