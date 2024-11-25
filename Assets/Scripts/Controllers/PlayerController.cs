using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerController : CreatureController
{
    public int EquipProtectsCount
    {
        get
        {
            int count = 0;

            if (Items.ContainsKey("Glove"))
                count++;

            if (Items.ContainsKey("Goggle"))
                count++;

            if (Items.ContainsKey("Mask"))
                count++;

            if (Items.ContainsKey("ProtectedGear"))
                count++;

            return count;
        }
    }
}
