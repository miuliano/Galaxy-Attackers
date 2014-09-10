public struct IntVector2 {

	int _x;

	public int x
	{
		get
		{
			return _x;
		}
		set
		{
			_x = value;
		}
	}

	int _y;

	public int y
	{
		get
		{
			return _y;
		}
		set
		{
			_y = value;
		}
	}

	public int sqrMagnitude
	{
		get
		{
			return (_x * _x) + (_y * _y);
		}
	}

	public int SqrMagnitude()
	{
		return (_x * _x) + (_y * _y);
	}

	public IntVector2(int x, int y)
	{
		_x = x;
		_y = y;
	}

	public void Set(int new_x, int new_y)
	{
		_x = new_x;
		_y = new_y;
	}

	public override string ToString()
	{
		return string.Format("({0},{1})", _x, _y);
	}

	public static IntVector2 operator +(IntVector2 a, IntVector2 b)
	{
		return new IntVector2(a.x + b.x, a.y + b.y);
	}

	public static IntVector2 operator -(IntVector2 a, IntVector2 b)
	{
		return new IntVector2(a.x - b.x, a.y - b.y);
	}

	public static IntVector2 operator *(IntVector2 a, int b)
	{
		return new IntVector2(a.x * b, a.y * b);
	}

	public static bool operator ==(IntVector2 a, IntVector2 b)
	{
		return (a.x == b.x && a.y == b.y);
	}

	public static bool operator !=(IntVector2 a, IntVector2 b)
	{
		return (a.x != b.x || a.y != b.y);
	}
}
