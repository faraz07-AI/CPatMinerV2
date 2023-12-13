package transformation;

import org.eclipse.jdt.core.dom.AST;
import org.eclipse.jdt.core.dom.StringLiteral;
import org.eclipse.jdt.core.dom.Expression;

import java.util.Objects;

public class TransformationUtils {

    static String capitalizeFirstLetter(String input) {
        if (input == null || input.isEmpty())
            return input;
        return input.substring(0, 1).toUpperCase() + input.substring(1);
    }

    public static boolean isBoolean(String input) {
        return Objects.equals(input, "true") || Objects.equals(input, "false");
    }

    public static boolean isInteger(String input) {
        try {
            Integer.parseInt(input);
            return true;
        } catch (NumberFormatException e) {
            return false;
        }
    }

    static Expression type_literal(AST asn, String input) {
        if (isBoolean(input)) {
            return asn.newBooleanLiteral(Boolean.parseBoolean(input));
        } else if (isInteger(input)) {
            return asn.newNumberLiteral(input);
        } else {
            StringLiteral stringLiteral = asn.newStringLiteral();
            stringLiteral.setLiteralValue(input);
            return stringLiteral;
        }
    }
}
