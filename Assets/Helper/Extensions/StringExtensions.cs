public static class StringExtensions
{
    
    public static string replaceData(this string text, KVStorage data, string separator = "{}")
    {
        if (text.Contains(separator[0]) && text.Contains(separator[1]) && data != null)
        {
            text = text.Replace(separator[0].ToString(), separator[0] + "!");
            text = text.Replace(separator[1], separator[0]);

            string[] stringSeperate = text.Split(separator[0]);

            string value = "";

            foreach (string st in stringSeperate)
            {
                string stt = st;
                if (stt.StartsWith("!"))
                {
                    string old = stt.Substring(1);
                    bool leadingUpper = false;
                    bool allUpper = false;
                    bool leadingLower = false;
                    bool allLower = false;

                    if (stt.StartsWith("!++"))
                    {
                        allUpper = true;
                        stt = stt.Replace("!++", "!");
                    }
                    else if (stt.StartsWith("!+"))
                    {
                        leadingUpper = true;
                        stt = stt.Replace("!+", "!");
                    }
                    else if (stt.StartsWith("!--"))
                    {
                        allLower = true;
                        stt = stt.Replace("!--", "!");
                    }
                    else if (stt.StartsWith("!-"))
                    {
                        leadingLower = true;
                        stt = stt.Replace("!-", "!");
                    }

                    stt = stt.Replace("!", "");

                    if (!data.getElement(stt, ref stt))
                        stt = separator[0] + old + separator[1];

                    if (allUpper)
                        stt = stt.ToUpper();
                    else if (leadingUpper)
                    {
                        if (stt.Length > 1)
                            stt = stt.Substring(0, 1).ToUpper() + stt.Substring(1);
                        else
                            stt = stt.ToUpper();
                    }
                    else if (allLower)
                        stt = stt.ToLower();
                    else if (leadingLower)
                    {
                        if (stt.Length > 1)
                            stt = stt.Substring(0, 1).ToLower() + stt.Substring(1);
                        else
                            stt = stt.ToLower();
                    }
                }
                value += stt;
            }

            text = value;

            //text = text.replaceData(data, separator);
        }

        return text;
    }
}
