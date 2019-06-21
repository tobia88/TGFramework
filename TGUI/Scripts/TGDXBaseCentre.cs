using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TGDXBaseCentre: MonoBehaviour {

    public bool isActive { get; protected set; }

    public virtual void Init() {
       SetActive( false ); 
    }

    public virtual void SetActive( bool _active ) {
        isActive = _active;

        gameObject.SetActive( isActive );
    }
}
