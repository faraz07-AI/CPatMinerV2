package transformation;

import org.eclipse.jdt.core.dom.*;
import org.eclipse.jdt.core.dom.Block;

import java.util.ArrayList;
import java.util.List;
import java.util.Objects;

import com.github.gumtreediff.tree.Tree;

public class SrcMLToJavaASTTransformation {

    public static ArrayList<?> getNodeMapping(AST asn, Tree node ){

        if (Objects.equals(node.getType().toString(), SrcMLNodeType.FUNCTION)){
            MethodDeclaration methoddec = asn.newMethodDeclaration();
            // function name
            List<Tree> children = node.getChildren();
            String function_name = children.get(1).getLabel();
            SimpleName methodName = asn.newSimpleName(function_name);
            methoddec.setName(methodName);
            // function return type
            String return_type = "";
            for(Tree c : children.get(0).getChildren()){
                if (Objects.equals(c.getType().toString(), SrcMLNodeType.NAME)){
                    return_type = TransformationUtils.capitalizeFirstLetter(c.getLabel());
                    methoddec.setReturnType(asn.newSimpleType(asn.newSimpleName(return_type)));
                    break;
                }
            }
            // function params
            List<Tree> params = children.get(2).getChildren();
            for( Tree param: params){
                List<Tree> param_children = param.getChildren().get(0).getChildren(); // decl obj
                String param_type = TransformationUtils.capitalizeFirstLetter(param_children.get(0).getChildren().get(0).getLabel());
                String param_name = param_children.get(1).getLabel();
                SingleVariableDeclaration parameter = asn.newSingleVariableDeclaration();
                SimpleType simpleType = asn.newSimpleType(asn.newSimpleName(param_type));
                parameter.setType(simpleType);
                SimpleName simpleName = asn.newSimpleName(param_name);
                parameter.setName(simpleName);
                parameter.setSourceRange(param.getPos(), param.getLength());
                methoddec.parameters().add(parameter);
            }

            // function body
            Block methodBody = asn.newBlock();
            List<Tree> function_block_statements = children.get(3).getChildren().get(0).getChildren();
            for (Tree statements: function_block_statements){
                // TODO treat case if there is "new"
                if (Objects.equals(statements.getType().toString(), SrcMLNodeType.DECL_STMT)){
                    List<Tree> declaration = statements.getChildren().get(0).getChildren();
                    String var_type = TransformationUtils.capitalizeFirstLetter(declaration.get(0).getChildren().get(0).getLabel());
                    String var_name = declaration.get(1).getLabel();

                    VariableDeclarationFragment variableFragment = asn.newVariableDeclarationFragment();
                    variableFragment.setName(asn.newSimpleName(var_name));
                    if (Objects.equals(declaration.get(2).getType().toString(), SrcMLNodeType.INIT)) {
                        String init_literal = declaration.get(2).getChildren().get(0).getChildren().get(0).getLabel();
                        Expression type_literal = TransformationUtils.type_literal(asn, init_literal);
                        variableFragment.setInitializer(type_literal);
                    }
                    VariableDeclarationStatement variableDeclaration = asn.newVariableDeclarationStatement(variableFragment);
                    variableDeclaration.setType(asn.newSimpleType(asn.newSimpleName(var_type)));
                    methodBody.statements().add(variableDeclaration);

                }
                else if (Objects.equals(statements.getType().toString(), SrcMLNodeType.EXPR_STMT)){
                    List<Tree> expression = statements.getChildren().get(0).getChildren();
                    //assignement

                    if (Objects.equals(expression.get(0).getType().toString(), SrcMLNodeType.NAME)){
                        Assignment assignment = asn.newAssignment();
                        assignment.setLeftHandSide(asn.newSimpleName(expression.get(0).getLabel()));
                        InfixExpression infixExpression = asn.newInfixExpression();
                        String expression_var = expression.get(2).getLabel();
                        Expression leftExpression =  TransformationUtils.type_literal(asn, expression_var);
                        if (expression.size() > 3){
                            expression_var = expression.get(4).getLabel();
                            Expression rightExpression = TransformationUtils.type_literal(asn, expression_var);
                            infixExpression.setLeftOperand(leftExpression);
                            infixExpression.setRightOperand(rightExpression);
                            assignment.setRightHandSide(infixExpression);
                            // if it's:  a = a
                            if (Objects.equals(expression.get(3).getType().toString(), SrcMLNodeType.OPERATOR)) {
                                if (Objects.equals(expression.get(3).getLabel(), "*")) {
                                    infixExpression.setOperator(InfixExpression.Operator.TIMES);
                                } else if (Objects.equals(expression.get(3).getLabel(), "+")) {
                                    infixExpression.setOperator(InfixExpression.Operator.PLUS);
                                } else if (Objects.equals(expression.get(3).getLabel(), "-")) {
                                    infixExpression.setOperator(InfixExpression.Operator.MINUS);
                                } else if (Objects.equals(expression.get(3).getLabel(), "/")) {
                                    infixExpression.setOperator(InfixExpression.Operator.DIVIDE);
                                }
                            }
                        }
                        ExpressionStatement assignmentStatement = asn.newExpressionStatement(assignment);
                        methodBody.statements().add(assignmentStatement);
                    }
                    // function call
                    else if (Objects.equals(expression.get(0).getType().toString(), SrcMLNodeType.CALL)){
                        List<Tree> method_invocation_name = expression.get(0).getChildren().get(0).getChildren();
                        MethodInvocation methodInvocation = asn.newMethodInvocation();
                        methodInvocation.setExpression(asn.newSimpleName(method_invocation_name.get(0).getLabel()));
                        methodInvocation.setName(asn.newSimpleName(method_invocation_name.get(2).getLabel()));
                        // TODO treat case if there is multiple function invocation
                        List<Tree> args = expression.get(0).getChildren().get(1).getChildren();
                        for( Tree arg: args){
                            for (Tree s : arg.getChildren().get(0).getChildren())
                                if (Objects.equals(s.getType().toString(), SrcMLNodeType.LITERAL)){
                                    Expression helloWorldLiteral = TransformationUtils.type_literal(asn, s.getLabel());
                                    methodInvocation.arguments().add(helloWorldLiteral);
                                }
                        }

                        ExpressionStatement expressionStatement = asn.newExpressionStatement(methodInvocation);
                        methodBody.statements().add(expressionStatement);

                    }
                    // for loop
                    else if (Objects.equals(expression.get(0).getType().toString(), SrcMLNodeType.FOR)){
                        Tree call = expression.get(0);

                    }

                }
            }
            methoddec.setBody(methodBody );


            methoddec.setSourceRange(node.getPos(), node.getLength());
            ArrayList<MethodDeclaration> list_method = new ArrayList<>();
            list_method.add(methoddec);
            return list_method;
        }

        return null;

    }

}
