using System.Collections.Generic;

public class FixedSizeQueue<T>
{   
    private Queue<T> queue;
    public int Size{get; private set;}

    public FixedSizeQueue(int size){
        queue = new Queue<T>();
        Size = size;
    }

    public void Enqueue(T obj){
        if(queue.Count == Size){
            queue.Dequeue();
        }

        queue.Enqueue(obj);
    }

    public Queue<T>.Enumerator GetEnumerator(){
        return queue.GetEnumerator();
    }

    public int Count(){
        return queue.Count;
    }
}

