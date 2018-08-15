using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface ISuperballInstantiatedEvent : IEventSystemHandler {

    void SuperballIsBuilt();
    void SuperballIsDestroyed();
}
