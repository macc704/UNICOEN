class Person {
	String myName;
	int myAge;

	public void SetName(String name) {
		myName = name;
	}

	public String GetName() {
		return myName;
	}

	public void SetAge(int age) {
		myAge = age;
	}

	public int GetAge() {
		return myAge;
	}
}

class PersonTest {
	public static void main(String[] args) {
		Person tanaka = new Person(); // 田中さんオブジェクトを作る
		tanaka.SetName("Tanaka"); // 田中さんの名前を設定する
		tanaka.SetAge(26); // 田中さんの年齢を設定する

		Person suzuki = new Person(); // 鈴木さんオブジェクトを作る
		suzuki.SetName("Suzuki"); // 鈴木さんの名前を設定する
		suzuki.SetAge(32); // 鈴木さんの年齢を設定する

		System.out.println(tanaka.GetName());
		System.out.println(tanaka.GetAge());
		System.out.println(suzuki.GetName());
		System.out.println(suzuki.GetAge());
	}
}

class Person2 {
	String myName;
	int myAge;

	Person2(String name) {
		myName = name;
	}

	public void SetName(String name) {
		myName = name;
	}

	public String GetName() {
		return myName;
	}

	public void SetAge(int age) {
		myAge = age;
	}

	public int GetAge() {
		return myAge;
	}
}

class Member extends Person {
	int myNumber;

	public void SetNumber(int number) {
		myNumber = number;
	}

	public int GetNumber() {
		return myNumber;
	}
}

class Member2 extends Person2 {
	Member2() {
		super("tanaka"); // 親クラスのコンストラクタを呼び出す
		super.SetAge(26); // 親クラスのメソッドを呼び出す
		String s = super.myName; // 親クラスの属性を参照する
	}
}