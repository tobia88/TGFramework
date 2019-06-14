using System;
using System.Collections;

using UnityEngine;

public static class CoroutineExtensions {
    public static Coroutine StartThrowCoroutine( this MonoBehaviour _monoBehaviour, IEnumerator _enumerator, Action<Exception> _exceptionCallback) {
        return _monoBehaviour.StartCoroutine( RunThrowingIterator( _enumerator, _exceptionCallback ));
    }

    private static IEnumerator RunThrowingIterator( IEnumerator _enumerator, Action<Exception> _exception ) {
        while( true ) {
            object current;

            try {
                if( _enumerator.MoveNext() == false ) {
                    break;
                }

                current = _enumerator.Current;
            }
            catch( Exception e ) {
                _exception( e );
                yield break;
            }

            yield return current;
        }

        _exception( null );
    }
}