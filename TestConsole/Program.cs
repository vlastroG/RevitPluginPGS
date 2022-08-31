List<(string, string)> list = new List<(string, string)>()
{
    ("123","3434343"),
    ("123","7634343"),
    ("123","3-0984343"),
    ("123","e78u74343"),
    ("123","09343"),
    ("123","48674343"),
    ("123","-0984343"),
    ("123","78474343"),
    ("321","78474343"),
    ("321","78474343"),
    ("321","78474343"),
    ("123","78474343"),
    ("123","78474343"),
    ("444","78474343")
};

var list2 = list.GroupBy(tuple => tuple.Item1);
foreach (var item in list2)
{
    if (item.Count() > 1)
    {
        foreach (var miniItem in item)
        {
            var t = 0;
        }
    }
    else
    {
        var b = 2;
    }
}

