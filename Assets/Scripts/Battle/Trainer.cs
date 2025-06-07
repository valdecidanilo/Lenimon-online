
public class Trainer
{
    public string name;//you | opponent
    public string referenceText;//you => allied | opponent => opponent's
    public Bag bag = new();
    public Pokemon[] party;
    public Pokemon activePokemon;

    public MoveModel pickedMove;
}
