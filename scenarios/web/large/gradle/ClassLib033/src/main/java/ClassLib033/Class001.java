package ClassLib033;

public class Class001 {
    public static String property() {
        return "ClassLib033" + " " + ClassLib002.Class001.property() + " " + ClassLib022.Class001.property() + " " + ClassLib012.Class001.property();
    }
}
