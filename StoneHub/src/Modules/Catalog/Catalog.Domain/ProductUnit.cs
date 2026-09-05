namespace Catalog.Domain;

// How a listing's quantity/price is measured. Extensible — add members as new categories need them.
public enum ProductUnit
{
    Piece,
    SquareFoot,
    SquareMeter,
    Slab,
    Box,
    Tonne,
    RunningFoot,
    Set
}
