package transformation;

import com.github.gumtreediff.tree.Tree;
import org.eclipse.jdt.core.dom.*;

import java.util.ArrayList;
import java.util.List;
import java.util.Objects;

public class SrcMLTreeVisitor {

    AST asn = AST.newAST(AST.JLS8);

    void visit(CommentNode node) {
        // Do nothing since comments are not needed
    }

    void visit(UsingNode node) {
        // No need to map it or its children
    }

    Expression visit(LiteralNode node) {
        String init_literal = node.getLabel();
        return TransformationUtils.type_literal(asn, init_literal);
    }

    SimpleName visit(NameNode node) {
        return asn.newSimpleName(node.getLabel());
    }

    SimpleType visitType(NameNode node) {
        return asn.newSimpleType(asn.newSimpleName(TransformationUtils.capitalizeFirstLetter(node.getLabel())));
    }

    MethodInvocation visit(CallNode node) {
        MethodInvocation methodInvocation = asn.newMethodInvocation();
        List<Tree> children = node.getChildren();
        Tree nameNode = children.get(0);
        if (nameNode.getChildren().size() == 1) { // if we only have one method call like do(...)
            Tree child = nameNode.getChildren().get(0);
            if (child instanceof NameNode) {
                methodInvocation.setName(this.visit((NameNode) child));
            }
        } else { // we have something like obj.do()
            List<Tree> method_calls = nameNode.getChildren();
            if (method_calls.get(0) instanceof NameNode)
                methodInvocation.setExpression(this.visit((NameNode) method_calls.get(0)));
            if (method_calls.get(2) instanceof NameNode)
                methodInvocation.setName(this.visit((NameNode) method_calls.get(2)));
        }
        Tree argNode = children.get(1);
        if (argNode instanceof ArgumentListNode) {
            for (Expression exp : this.visit((ArgumentListNode) argNode))
                methodInvocation.arguments().add(exp);
        }
        return methodInvocation;
    }

    Expression visit(ExprNode node) {
        List<Tree> children = node.getChildren();
        if (children.size() == 1) {
            if (children.get(0) instanceof LiteralNode)
                return this.visit((LiteralNode) children.get(0));
            if (children.get(0) instanceof NameNode)
                return this.visit((NameNode) children.get(0));
            if (children.get(0) instanceof CallNode)
                return this.visit((CallNode) children.get(0));
        } else if (children.size() == 2) {
            if (Objects.equals(children.get(0).getLabel(), "new")) {// expression with new MyClass(...)
                ClassInstanceCreation classInstanceCreation = asn.newClassInstanceCreation();
                if (children.get(1) instanceof CallNode) {
                    children = children.get(1).getChildren();
                    if (children.get(0) instanceof NameNode) {// Class name
                        SimpleType type = this.visitType((NameNode) children.get(0));
                        classInstanceCreation.setType(type);
                    }
                    if (children.get(1) instanceof ArgumentListNode) {
                        for (Expression exp : this.visit((ArgumentListNode) children.get(1)))
                            classInstanceCreation.arguments().add(exp);
                    }
                }
                return classInstanceCreation;
            } else if (TransformationUtils.isPostfix(children.get(1))) { // a++ or a--
                PostfixExpression postfixExpression = asn.newPostfixExpression();
                if (children.get(0) instanceof NameNode) {
                    SimpleName s = this.visit((NameNode) children.get(0));
                    postfixExpression.setOperand(s);
                }
                if (children.get(1) instanceof OperatorNode)
                    postfixExpression.setOperator(this.visitPostfix((OperatorNode) children.get(1)));
                return postfixExpression;
            }
        } else if (children.size() == 3) { // expression with one operator a+b or assig a+= 5
            if (children.get(1) instanceof OperatorNode && TransformationUtils.isAssignment(children.get(1))) {
                Assignment assignment = asn.newAssignment();
                if (children.get(0) instanceof NameNode)
                    assignment.setLeftHandSide(this.visit((NameNode) children.get(0)));
                if (children.get(1) instanceof OperatorNode)
                    assignment.setOperator(this.visitAssig((OperatorNode) children.get(1)));
                if (children.get(2) instanceof NameNode)
                    assignment.setRightHandSide(this.visit((NameNode) children.get(2)));
                else if (children.get(2) instanceof LiteralNode)
                    assignment.setRightHandSide(this.visit((LiteralNode) children.get(2)));
                return assignment;
            } else {
                InfixExpression infixExpression = asn.newInfixExpression();
                if (children.get(0) instanceof LiteralNode)
                    infixExpression.setLeftOperand(this.visit((LiteralNode) children.get(0)));
                else if (children.get(0) instanceof NameNode) // variable name
                    infixExpression.setLeftOperand(this.visit((NameNode) children.get(0)));
                if (children.get(1) instanceof OperatorNode)
                    infixExpression.setOperator(this.visit((OperatorNode) children.get(1)));
                if (children.get(2) instanceof LiteralNode)
                    infixExpression.setRightOperand(this.visit((LiteralNode) children.get(2)));
                else if (children.get(2) instanceof NameNode) // variable
                    infixExpression.setRightOperand(this.visit((NameNode) children.get(2)));
                return infixExpression;
            }
        } else if (children.size() > 3) { // expression with one or more operators a+b-c*d
            if (children.get(1) instanceof OperatorNode && TransformationUtils.isAssignment(children.get(1))) { // assignement
                Assignment assignment = asn.newAssignment();
                if (children.get(0) instanceof NameNode)
                    assignment.setLeftHandSide(this.visit((NameNode) children.get(0)));
                if (children.get(1) instanceof OperatorNode)
                    assignment.setOperator(this.visitAssig((OperatorNode) children.get(1)));
                // Since after the variable and the assign operator, we pretty much just have an Expr and we just want a InfixExpression , we can just visit
                ExprNode copy_without_assig = new ExprNode(node);
                // copy all children but remove the first two elements
                for (int i = 2; i < children.size(); i++)
                    copy_without_assig.addChild(children.get(i));
                assignment.setRightHandSide(this.visit(copy_without_assig));
                return assignment;
            }
            Expression result;
            if (children.get(0) instanceof LiteralNode)
                result = this.visit((LiteralNode) children.get(0));
            else
                result = this.visit((NameNode) children.get(0));
            for (int i = 1; i < children.size() - 1; i += 2) {
                InfixExpression infixExpression = asn.newInfixExpression();
                infixExpression.setLeftOperand(result);
                if (children.get(i) instanceof OperatorNode)
                    infixExpression.setOperator(this.visit((OperatorNode) children.get(i)));
                if (children.get(i + 1) instanceof LiteralNode)
                    infixExpression.setRightOperand(this.visit((LiteralNode) children.get(i + 1)));
                else if (children.get(i + 1) instanceof NameNode) // variable
                    infixExpression.setRightOperand(this.visit((NameNode) children.get(i + 1)));
                result = infixExpression;
            }
            return result;
        }
        return null;
    }

    Modifier visit(SpecifierNode node) {
        Modifier.ModifierKeyword modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.PUBLIC))
            modif = Modifier.ModifierKeyword.PUBLIC_KEYWORD;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.STATIC))
            modif = Modifier.ModifierKeyword.STATIC_KEYWORD;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.CONST))
            modif = Modifier.ModifierKeyword.FINAL_KEYWORD;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.EXPLICIT))
            modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.IN))
            modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.ASYNC))
            modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.EXTERN))
            modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.IMPLICIT))
            modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.INTERNAL))
            modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.OUT))
            modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.OVERRIDE))
            modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.PARAMS))
            modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.PARAMS))
            modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.PARTIAL))
            modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.PRIVATE))
            modif = Modifier.ModifierKeyword.PRIVATE_KEYWORD;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.PROTECTED))
            modif = Modifier.ModifierKeyword.PROTECTED_KEYWORD;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.READONLY))
            modif = Modifier.ModifierKeyword.FINAL_KEYWORD;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.REF))
            modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.SEALED))
            modif = Modifier.ModifierKeyword.FINAL_KEYWORD;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.STACKALLOC))
            modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.VIRTUAL))
            modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.VOLATILE))
            modif = Modifier.ModifierKeyword.VOLATILE_KEYWORD;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.YIELD))
            modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.THIS))
            modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.NEW))
            modif = null;
        if (Objects.equals(node.getLabel(), SrcMLNodeSpecifier.ABSTRACT))
            modif = Modifier.ModifierKeyword.ABSTRACT_KEYWORD;
        return asn.newModifier(modif);
    }

    InfixExpression.Operator visit(OperatorNode node) {
        if (Objects.equals(node.getLabel(), "*")) {
            return InfixExpression.Operator.TIMES;
        } else if (Objects.equals(node.getLabel(), "+")) {
            return InfixExpression.Operator.PLUS;
        } else if (Objects.equals(node.getLabel(), "-")) {
            return InfixExpression.Operator.MINUS;
        } else if (Objects.equals(node.getLabel(), "/")) {
            return InfixExpression.Operator.DIVIDE;
        } else if (Objects.equals(node.getLabel(), "%")) {
            return InfixExpression.Operator.REMAINDER;
        } else if (Objects.equals(node.getLabel(), ".")) {
            return null; // do nthg
        } else if (Objects.equals(node.getLabel(), "new")) {
            return null; // do nthg
        } else if (Objects.equals(node.getLabel(), "==")) {
            return InfixExpression.Operator.EQUALS;
        } else if (Objects.equals(node.getLabel(), "!=")) {
            return InfixExpression.Operator.NOT_EQUALS;
        } else if (Objects.equals(node.getLabel(), "(")) {
            return null; // TODO
        } else if (Objects.equals(node.getLabel(), ")")) {
            return null; // TODO
        } else if (Objects.equals(node.getLabel(), ">")) {
            return InfixExpression.Operator.GREATER;
        } else if (Objects.equals(node.getLabel(), "<")) {
            return InfixExpression.Operator.LESS;
        } else if (Objects.equals(node.getLabel(), ">=")) {
            return InfixExpression.Operator.GREATER_EQUALS;
        } else if (Objects.equals(node.getLabel(), "<=")) {
            return InfixExpression.Operator.LESS_EQUALS;
        } else if (Objects.equals(node.getLabel(), "&")) {
            return InfixExpression.Operator.AND;
        } else if (Objects.equals(node.getLabel(), "<<")) {
            return InfixExpression.Operator.LEFT_SHIFT;
        } else if (Objects.equals(node.getLabel(), ">>")) {
            return InfixExpression.Operator.RIGHT_SHIFT_SIGNED;
        } else if (Objects.equals(node.getLabel(), "|")) {
            return InfixExpression.Operator.OR;
        } else if (Objects.equals(node.getLabel(), ">>>")) {
            return InfixExpression.Operator.RIGHT_SHIFT_UNSIGNED;
        } else if (Objects.equals(node.getLabel(), "^")) {
            return InfixExpression.Operator.XOR;
        } else if (Objects.equals(node.getLabel(), "&&")) {
            return InfixExpression.Operator.CONDITIONAL_AND;
        } else if (Objects.equals(node.getLabel(), "||")) {
            return InfixExpression.Operator.CONDITIONAL_OR;
        } else
            return null;
    }

    Assignment.Operator visitAssig(OperatorNode node) {
        if (Objects.equals(node.getLabel(), "=")) {
            return Assignment.Operator.ASSIGN;
        } else if (Objects.equals(node.getLabel(), "*=")) {
            return Assignment.Operator.TIMES_ASSIGN;
        } else if (Objects.equals(node.getLabel(), "-=")) {
            return Assignment.Operator.MINUS_ASSIGN;
        } else if (Objects.equals(node.getLabel(), "+=")) {
            return Assignment.Operator.PLUS_ASSIGN;
        } else if (Objects.equals(node.getLabel(), "/=")) {
            return Assignment.Operator.DIVIDE_ASSIGN;
        } else if (Objects.equals(node.getLabel(), "%=")) {
            return Assignment.Operator.REMAINDER_ASSIGN;
        } else if (Objects.equals(node.getLabel(), "&=")) {
            return Assignment.Operator.BIT_AND_ASSIGN;
        } else if (Objects.equals(node.getLabel(), "^=")) {
            return Assignment.Operator.BIT_XOR_ASSIGN;
        } else if (Objects.equals(node.getLabel(), "|=")) {
            return Assignment.Operator.BIT_OR_ASSIGN;
        } else if (Objects.equals(node.getLabel(), "<<=")) {
            return Assignment.Operator.LEFT_SHIFT_ASSIGN;
        } else if (Objects.equals(node.getLabel(), ">>=")) {
            return Assignment.Operator.RIGHT_SHIFT_SIGNED_ASSIGN;
        } else if (Objects.equals(node.getLabel(), ">>>=")) {
            return Assignment.Operator.RIGHT_SHIFT_UNSIGNED_ASSIGN;
        } else
            return null;
    }

    PostfixExpression.Operator visitPostfix(OperatorNode node) {
        if (Objects.equals(node.getLabel(), "--")) {
            return PostfixExpression.Operator.DECREMENT;
        } else if (Objects.equals(node.getLabel(), "++")) {
            return PostfixExpression.Operator.INCREMENT;
        } else
            return null;
    }

    List<Expression> visit(ArgumentListNode node) {
        List<Expression> results = new ArrayList<>();
        List<Tree> args = node.getChildren();
        for (Tree arg : args) {
            if (arg instanceof ArgumentNode) { // case of (a,b), need to check: (a,a+b) or (a,new class(xy,xyd))
                results.add(this.visit((ArgumentNode) arg));
            }
        }
        return results;
    }

    Expression visit(ArgumentNode node) {
        Tree expr = node.getChildren().get(0);
        if (expr instanceof ExprNode) {
            return this.visit((ExprNode) expr);
        }
        return null;
    }

    MethodDeclaration visit(FunctionDeclNode node) {
        MethodDeclaration methoddec = asn.newMethodDeclaration();
        // function name
        List<Tree> children = node.getChildren();
        if (children.get(1) instanceof NameNode)
            methoddec.setName(this.visit((NameNode) children.get(1)));

        List<Tree> types = children.get(0).getChildren();
        // function specifier
        for (int i = 0; i < types.size() - 1; i++) {
            Tree specifier = types.get(i);
            if (specifier instanceof SpecifierNode)
                methoddec.modifiers().add(this.visit((SpecifierNode) specifier));
        }
        // function return type
        Tree return_node = types.get(types.size() - 1); // the return type is the last one, the others are specifiers
        if (return_node instanceof NameNode)
            methoddec.setReturnType2(this.visitType((NameNode) return_node));
        // function params
        if (children.get(2) instanceof ParameterListNode) {
            for (VariableDeclaration vdec : this.visit((ParameterListNode) children.get(2)))
                methoddec.parameters().add(vdec);
        }
        return methoddec;
    }

    MethodDeclaration visit(FunctionNode node) {
        MethodDeclaration methoddec = asn.newMethodDeclaration();
        // function name
        List<Tree> children = node.getChildren();
        if (children.get(1) instanceof NameNode)
            methoddec.setName(this.visit((NameNode) children.get(1)));

        List<Tree> types = children.get(0).getChildren();
        // function specifier
        for (int i = 0; i < types.size() - 1; i++) {
            Tree specifier = types.get(i);
            if (specifier instanceof SpecifierNode)
                methoddec.modifiers().add(this.visit((SpecifierNode) specifier));
        }
        // function return type
        Tree return_node = types.get(types.size() - 1); // the return type is the last one, the others are specifiers
        if (return_node instanceof NameNode)
            methoddec.setReturnType2(this.visitType((NameNode) return_node));
        // function params
        if (children.get(2) instanceof ParameterListNode) {
            for (VariableDeclaration vdec : this.visit((ParameterListNode) children.get(2)))
                methoddec.parameters().add(vdec);
        }
        // function body
        if (children.get(3) instanceof BlockNode) {
            methoddec.setBody((Block) this.visit((BlockNode) children.get(3)));
        }
        return methoddec;
    }

    List<VariableDeclaration> visit(ParameterListNode node) {
        List<VariableDeclaration> results = new ArrayList<>();
        List<Tree> params = node.getChildren();
        for (Tree param : params) {
            if (param instanceof ParameterNode) {
                results.add(this.visit((ParameterNode) param));
            }
        }
        return results;
    }

    VariableDeclaration visit(ParameterNode node) {
        SingleVariableDeclaration parameter = asn.newSingleVariableDeclaration();
        if (node.getChildren().get(0) instanceof DeclNode) {
            List<Tree> param_children = node.getChildren().get(0).getChildren();
            Tree node_type = param_children.get(0);
            if (node_type instanceof TypeNode) {
                parameter.setType(this.visit((TypeNode) node_type));
            }

            Tree node_name = param_children.get(1);
            if (node_name instanceof NameNode)
                parameter.setName(this.visit((NameNode) node_name));
            parameter.setSourceRange(node.getPos(), node.getLength());
        }

        return parameter;
    }

    Type visit(TypeNode node) {
        Tree node_type = node.getChildren().get(0);
        if (node_type.getChildren().size() > 0) {// an array type type->name->[name:..,index:..]
            Tree param_type = node_type.getChildren().get(0);
            if (node_type.getChildren().size() > 1) { // an array type
                Tree arr_index = node_type.getChildren().get(1);
                if (arr_index.getType().toString() == "index") {
                    SimpleType simpleType = this.visitType((NameNode) param_type);
                    ArrayType arrayType = asn.newArrayType(simpleType);
                    return arrayType;
                }
            }
        } else if (node_type instanceof NameNode) { // type->name:...
            SimpleType simpleType = this.visitType((NameNode) node_type);
            return simpleType;
        }
        return null;
    }

    Object visit(BlockNode node) {
        //Block body = asn.newBlock();
        Tree child = node.getChildren().get(0);
        if (child instanceof BlockContentNode)
            return this.visit((BlockContentNode) child);
        if (child instanceof ClassNode){
            List<TypeDeclaration> class_nodes = new ArrayList<>();
            for (Tree class_node : node.getChildren()) {
                if (class_node instanceof ClassNode)
                    class_nodes.add(this.visit((ClassNode) class_node));
            }
            return class_nodes;
        }

        List<MethodDeclaration> functions = new ArrayList<>();
        for (Tree function : node.getChildren()) {
            if (function instanceof FunctionNode)
                functions.add(this.visit((FunctionNode) function));
            else if (function instanceof FunctionDeclNode)
                functions.add(this.visit((FunctionDeclNode) function));
        }
        return functions;
    }

    Block visit(BlockContentNode node) {
        Block body = asn.newBlock();
        List<Tree> statements = node.getChildren();
        for (Tree statement : statements) {
            if (statement instanceof DeclStmtNode) {
                body.statements().add(this.visit((DeclStmtNode) statement));
            } else if (statement instanceof ExprStmtNode) {
                ExpressionStatement k = this.visit((ExprStmtNode) statement);
                body.statements().add(k);
            } else if (statement instanceof ReturnNode) {
                ReturnStatement k = this.visit((ReturnNode) statement);
                body.statements().add(k);
            }
        }
        return body;
    }

    VariableDeclarationStatement visit(DeclStmtNode node) {
        Tree declaration = node.getChildren().get(0);
        if (declaration instanceof DeclNode)
            return this.visit((DeclNode) declaration);
        return null;
    }

    VariableDeclarationStatement visit(DeclNode node) {
        List<Tree> declaration = node.getChildren();
        VariableDeclarationFragment variableFragment = asn.newVariableDeclarationFragment();
        if (declaration.get(1) instanceof NameNode) {
            variableFragment.setName(this.visit((NameNode) declaration.get(1)));
        }
        if (declaration.size() > 2 && declaration.get(2) instanceof InitNode) {
            Expression type_literal = this.visit((InitNode) declaration.get(2));
            variableFragment.setInitializer(type_literal);
        }
        VariableDeclarationStatement variableDeclaration = asn.newVariableDeclarationStatement(variableFragment);
        if (declaration.get(0) instanceof TypeNode) {
            Type t = this.visit((TypeNode) declaration.get(0));
            variableDeclaration.setType(t);
        }
        return variableDeclaration;
    }

    Expression visit(InitNode node) {
        if (node.getChildren().get(0) instanceof ExprNode)
            return this.visit((ExprNode) node.getChildren().get(0));
        return null;
    }

    ExpressionStatement visit(ExprStmtNode node) {
        if (node.getChildren().get(0) instanceof ExprNode) {
            Expression exp = this.visit((ExprNode) node.getChildren().get(0));
            return asn.newExpressionStatement(exp);
        }
        return null;
    }

    TransformationUtils.ReturnPair<PackageDeclaration, List<TypeDeclaration>> visit(NamespaceNode node) {

        List<Tree> children = node.getChildren();
        PackageDeclaration packageDeclaration = asn.newPackageDeclaration();;
        List<TypeDeclaration> classes = null;

        if (children.get(0) instanceof NameNode)
            packageDeclaration.setName(this.visit((NameNode) children.get(0)));
        if (children.get(1) instanceof BlockNode){
            classes = (List<TypeDeclaration>) this.visit((BlockNode) children.get(1) );
        }
        return new TransformationUtils.ReturnPair<>(packageDeclaration, classes);
    }

    CompilationUnit visit(UnitNode node) {
        CompilationUnit compilationUnit = asn.newCompilationUnit();
        for(Tree child:node.getChildren()){
            if(child instanceof UsingNode){
                this.visit((UsingNode)child); // does nothing? or imports?
            }
            else if(child instanceof NamespaceNode){
                TransformationUtils.ReturnPair<PackageDeclaration , List<TypeDeclaration>> results = this.visit((NamespaceNode) child);
                compilationUnit.setPackage(results.getFirst());
                for(TypeDeclaration t:results.getSecond()){
                    compilationUnit.types().add(t);
                }
            }
        }
        return compilationUnit;
    }

    void visit(OnNode node) {
    }

    void visit(IfNode node) {

    }

    TypeDeclaration visit(ClassNode node) {
        List<Tree> children = node.getChildren();
        TypeDeclaration classDeclaration = asn.newTypeDeclaration();
        for (Tree child : children) {
            if (child instanceof SpecifierNode) {
                classDeclaration.modifiers().add(this.visit((SpecifierNode) child));
            } else if (child instanceof NameNode) {
                classDeclaration.setName(this.visit((NameNode) child));
            } else if (child instanceof SuperListNode) {
                List<SimpleType> types = this.visit((SuperListNode) child);
                if (types.size() > 1){
                    for (SimpleType t : types) {
                        classDeclaration.superInterfaceTypes().add(t);
                    }
                }
                else if (types.size() == 1){
                    classDeclaration.setSuperclassType(types.get(0));
                }
            } else if (child instanceof BlockNode) {
                List<MethodDeclaration> lm = (List<MethodDeclaration>) this.visit((BlockNode) child);
                for (Object b : lm)
                    classDeclaration.bodyDeclarations().add(b);
            }
        }

        return classDeclaration;
    }

    void visit(EscapeNode node) {
    }

    void visit(BreakNode node) {
    }

    void visit(CaseNode node) {
    }

    void visit(ContinueNode node) {
    }

    void visit(DefaultNode node) {
    }

    void visit(DoNode node) {
    }

    void visit(EmptyStmtNode node) {
    }

    void visit(FixedNode node) {
    }

    void visit(ForNode node) {
    }

    void visit(ForeachNode node) {
    }

    void visit(GotoNode node) {
    }

    void visit(IfStmtNode node) {
    }

    void visit(LabelNode node) {
    }

    void visit(LockNode node) {
    }

    ReturnStatement visit(ReturnNode node) {
        ReturnStatement returnStatement = asn.newReturnStatement();
        if (node.getChildren().get(0) instanceof ExprNode)
            returnStatement.setExpression(this.visit((ExprNode) node.getChildren().get(0)));
        //methodBody.statements().add(returnStatement);
        return returnStatement;
    }

    void visit(SwitchNode node) {
    }

    void visit(UnsafeNode node) {
    }

    void visit(UsingStmtNode node) {
    }

    void visit(WhileNode node) {
    }

    void visit(ConditionNode node) {
    }

    void visit(ControlNode node) {
    }

    void visit(ElseNode node) {
    }

    void visit(IncrNode node) {
    }

    void visit(ThenNode node) {
    }

    void visit(DelegateNode node) {
    }

    void visit(LambdaNode node) {
    }

    void visit(ModifierNode node) {
    }


    void visit(ConstructorNode node) {
    }

    void visit(DestrctorNode node) {
    }

    void visit(EnumNode node) {
    }

    void visit(EventNode node) {
    }

    List<SimpleType> visit(SuperListNode node) {
        List<SimpleType> supers = new ArrayList<>();
        for (Tree child : node.getChildren()) {
            if (child instanceof SuperNode) {
                supers.add(this.visit((SuperNode) child));
            }
        }
        return supers;
    }

    SimpleType visit(SuperNode node) {
        Tree name = node.getChildren().get(0);
        if (name instanceof NameNode)
            return this.visitType((NameNode) name);
        return null;
    }

    void visit(InterfaceNode node) {
    }

    void visit(PropertyNode node) {
    }

    void visit(StructNode node) {
    }

    void visit(TernaryNode node) {
    }

    void visit(AttributeNode node) {
    }

    void visit(CheckedNode node) {
    }

    void visit(TypeOfNode node) {
    }

    void visit(SizeOfNode node) {
    }

    void visit(UncheckedNode node) {
    }

    void visit(ConstraintNode node) {
    }

    void visit(CatchNode node) {
    }

    void visit(FinallyNode node) {
    }

    void visit(ThrowNode node) {
    }

    void visit(TryNode node) {
    }

    void visit(ByNode node) {
    }

    void visit(EqualsNode node) {
    }

    void visit(FromNode node) {
    }

    void visit(GroupNode node) {
    }

    void visit(InNode node) {
    }

    void visit(IntoNode node) {
    }

    void visit(JoinNode node) {
    }

    void visit(LetNode node) {
    }

    void visit(LinqNode node) {
    }

    void visit(OrderByNode node) {
    }

    void visit(SelectNode node) {
    }

    void visit(WhereNode node) {
    }
}
