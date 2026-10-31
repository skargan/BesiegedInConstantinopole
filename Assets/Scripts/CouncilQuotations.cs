using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

// Presentation only: campaign identity seeds this deck; Unity's gameplay RNG is never read or advanced.
public static class CouncilQuotations
{
    [Serializable] public class Quotation {
        public string id, text, attribution, displayAttribution, sourceId;
        public string[] labels, sourceUrls;
        public string DisplayText=>"<i>"+text+"</i>\n<size=85%>"+displayAttribution+"</size>";
    }
    [Serializable] private class Catalogue {
        public int schemaVersion, importedCount;
        public string catalogueVersion;
        public Quotation[] quotes;
    }
    private static Catalogue catalogue;
    private static string cachedIdentity;
    private static int[] cachedDeck;
    public static IReadOnlyList<Quotation> Entries {get{Load();return catalogue.quotes;}}

    private static void Load()
    {
        if(catalogue!=null)return;
        var asset=Resources.Load<TextAsset>("Data/council-quotes");
        if(asset==null)throw new InvalidOperationException("The council quotation catalogue is missing.");
        var parsed=JsonUtility.FromJson<Catalogue>(asset.text);
        if(parsed==null || parsed.schemaVersion!=1 || parsed.quotes==null || parsed.quotes.Length==0 || parsed.importedCount!=parsed.quotes.Length)
            throw new InvalidOperationException("Invalid council quotation catalogue.");
        var ids=new HashSet<string>();var words=new HashSet<string>();
        foreach(var quote in parsed.quotes) {
            if(quote==null || string.IsNullOrWhiteSpace(quote.id) || !ids.Add(quote.id) || string.IsNullOrWhiteSpace(quote.text) || !words.Add(quote.text) || string.IsNullOrWhiteSpace(quote.displayAttribution))
                throw new InvalidOperationException("Missing or duplicate council quotation.");
        }
        catalogue=parsed;
    }

    public static Quotation ForTurn(string campaignId,int turn)
    {
        Load();
        if(string.IsNullOrEmpty(campaignId))throw new ArgumentException("A campaign identity is required.",nameof(campaignId));
        turn=Math.Max(0,turn);
        int count=catalogue.quotes.Length;
        if(cachedDeck==null || cachedIdentity!=campaignId) {
            // One campaign-specific permutation gives a full non-repeating cycle even when a
            // campaign starts at the siege or resumes from a non-zero turn. Menu visits never reroll it.
            cachedDeck=Shuffle(campaignId,count);
            cachedIdentity=campaignId;
        }
        return catalogue.quotes[cachedDeck[turn%count]];
    }

    private static int[] Shuffle(string campaignId,int count)
    {
        ulong seed=0;
        using(var sha=SHA256.Create()) {
            var bytes=sha.ComputeHash(Encoding.UTF8.GetBytes("council-quotes/v1/"+catalogue.catalogueVersion+"/"+campaignId));
            for(int i=0;i<8;i++)seed|=(ulong)bytes[i]<<(8*i);
        }
        var random=new QuoteRandom(seed);
        var deck=new int[count];for(int i=0;i<count;i++)deck[i]=i;
        for(int i=count-1;i>0;i--){int j=random.Below(i+1);int entry=deck[i];deck[i]=deck[j];deck[j]=entry;}
        return deck;
    }

    // Defined arithmetic keeps the shuffle stable across processes and framework RNG implementations.
    private struct QuoteRandom {
        private ulong state;
        public QuoteRandom(ulong seed){state=seed==0?0x9E3779B97F4A7C15UL:seed;}
        private ulong Next(){unchecked{state^=state>>12;state^=state<<25;state^=state>>27;return state*2685821657736338717UL;}}
        public int Below(int bound){ulong limit=ulong.MaxValue-ulong.MaxValue%(ulong)bound,value;do{value=Next();}while(value>=limit);return (int)(value%(ulong)bound);}
    }
}
