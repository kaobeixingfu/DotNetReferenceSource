//------------------------------------------------------------------------------
// <copyright file="QilVisitor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace System.Xml.Xsl.Qil {

    /// <summary>A base internal class for QIL visitors.</summary>
    /// <remarks>
    /// <p>QilVisitor is a base internal class for traversing QIL graphs.  Override individual Visit methods to change
    /// behavior for only certain node types; override Visit() to change behavior for all node types at once; override
    /// VisitChildren() to change the algorithm for iterating and visiting children.</p>
    /// <p>Subclasses may also find it useful to annotate the tree during visitation.</p>
    /// </remarks>
    internal abstract class QilVisitor {

        //-----------------------------------------------
        // QilVisitor methods (manually generated)
        //-----------------------------------------------

        /// <summary>
        /// If a reference is passed to the Visit() method, it is assumed to be the definition.
        /// This method assumes it is a reference to a definition.
        /// For example, if a Let node is visited, is it the Let definition or a reference to the
        /// the Let definition?  Without context, it is ambiguous.  This method allows a caller
        /// to disambiguate.
        /// </summary>
        protected virtual QilNode VisitAssumeReference(QilNode expr) {
            if (expr is QilReference)
                return VisitReference(expr);

            return Visit(expr);
        }

        /// <summary>
        /// Visit all children of "parent".  By default, take care to avoid circular visits.
        /// </summary>
        protected virtual QilNode VisitChildren(QilNode parent) {
            for (int i = 0; i < parent.Count; i++) {
                // If child is a reference, then call VisitReference instead of Visit in order to avoid circular visits.
                if (IsReference(parent, i))
                    VisitReference(parent[i]);
                else
                    Visit(parent[i]);
            }

            return parent;
        }

        /// <summary>
        /// Visit all children of "parent".  Take care to avoid circular visits.
        /// </summary>
        protected virtual bool IsReference(QilNode parent, int childNum) {
            QilNode child = parent[childNum];

            if (child != null) {
                switch (child.NodeType) {
                    case QilNodeType.For:
                    case QilNodeType.Let:
                    case QilNodeType.Parameter:
                        // Is this a reference or a definition?
                        switch (parent.NodeType) {
                            case QilNodeType.Loop:
                            case QilNodeType.Filter:
                            case QilNodeType.Sort:
                                // Second child of these node types is a reference; first child is a definition
                                return childNum == 1;

                            case QilNodeType.GlobalVariableList:
                            case QilNodeType.GlobalParameterList:
                            case QilNodeType.FormalParameterList:
                                // All children of definition lists are definitions
                                return false;
                        }

                        // All other cases are references
                        return true;

                    case QilNodeType.Function:
                        // If parent is an Invoke node, then visit a reference to the function
                        return parent.NodeType == QilNodeType.Invoke;
                }
            }

            return false;
        }


        //-----------------------------------------------
        // QilVisitor methods (auto-generated)
        //-----------------------------------------------

        // Do not edit this region
        #region AUTOGENERATED
        protected virtual QilNode Visit(QilNode n) {
            if (n == null)
                return VisitNull();
            
            switch (n.NodeType) {
                case QilNodeType.QilExpression: return VisitQilExpression((QilExpression)n);
                case QilNodeType.FunctionList: return VisitFunctionList((QilList)n);
                case QilNodeType.GlobalVariableList: return VisitGlobalVariableList((QilList)n);
                case QilNodeType.GlobalParameterList: return VisitGlobalParameterList((QilList)n);
                case QilNodeType.ActualParameterList: return VisitActualParameterList((QilList)n);
                case QilNodeType.FormalParameterList: return VisitFormalParameterList((QilList)n);
                case QilNodeType.SortKeyList: return VisitSortKeyList((QilList)n);
                case QilNodeType.BranchList: return VisitBranchList((QilList)n);
                case QilNodeType.OptimizeBarrier: return VisitOptimizeBarrier((QilUnary)n);
                case QilNodeType.Unknown: return VisitUnknown(n);
                
                case QilNodeType.DataSource: return VisitDataSource((QilDataSource)n);
                case QilNodeType.Nop: return VisitNop((QilUnary)n);
                case QilNodeType.Error: return VisitError((QilUnary)n);
                case QilNodeType.Warning: return VisitWarning((QilUnary)n);
                
                case QilNodeType.For: return VisitFor((QilIterator)n);
                case QilNodeType.Let: return VisitLet((QilIterator)n);
                case QilNodeType.Parameter: return VisitParameter((QilParameter)n);
                case QilNodeType.PositionOf: return VisitPositionOf((QilUnary)n);
                
                case QilNodeType.True: return VisitTrue(n);
                case QilNodeType.False: return VisitFalse(n);
                case QilNodeType.LiteralString: return VisitLiteralString((QilLiteral)n);
                case QilNodeType.LiteralInt32: return VisitLiteralInt32((QilLiteral)n);
                case QilNodeType.LiteralInt64: return VisitLiteralInt64((QilLiteral)n);
                case QilNodeType.LiteralDouble: return VisitLiteralDouble((QilLiteral)n);
                case QilNodeType.LiteralDecimal: return VisitLiteralDecimal((QilLiteral)n);
                case QilNodeType.LiteralQName: return VisitLiteralQName((QilName)n);
                case QilNodeType.LiteralType: return VisitLiteralType((QilLiteral)n);
                case QilNodeType.LiteralObject: return VisitLiteralObject((QilLiteral)n);
                
                case QilNodeType.And: return VisitAnd((QilBinary)n);
                case QilNodeType.Or: return VisitOr((QilBinary)n);
                case QilNodeType.Not: return VisitNot((QilUnary)n);
                
                case QilNodeType.Conditional: return VisitConditional((QilTernary)n);
                case QilNodeType.Choice: return VisitChoice((QilChoice)n);
                
                case QilNodeType.Length: return VisitLength((QilUnary)n);
                case QilNodeType.Sequence: return VisitSequence((QilList)n);
                case QilNodeType.Union: return VisitUnion((QilBinary)n);
                case QilNodeType.Intersection: return VisitIntersection((QilBinary)n);
                case QilNodeType.Difference: return VisitDifference((QilBinary)n);
                case QilNodeType.Average: return VisitAverage((QilUnary)n);
                case QilNodeType.Sum: return VisitSum((QilUnary)n);
                case QilNodeType.Minimum: return VisitMinimum((QilUnary)n);
                case QilNodeType.Maximum: return VisitMaximum((QilUnary)n);
                
                case QilNodeType.Negate: return VisitNegate((QilUnary)n);
                case QilNodeType.Add: return VisitAdd((QilBinary)n);
                case QilNodeType.Subtract: return VisitSubtract((QilBinary)n);
                case QilNodeType.Multiply: return VisitMultiply((QilBinary)n);
                case QilNodeType.Divide: return VisitDivide((QilBinary)n);
                case QilNodeType.Modulo: return VisitModulo((QilBinary)n);
                
                case QilNodeType.StrLength: return VisitStrLength((QilUnary)n);
                case QilNodeType.StrConcat: return VisitStrConcat((QilStrConcat)n);
                case QilNodeType.StrParseQName: return VisitStrParseQName((QilBinary)n);
                
                case QilNodeType.Ne: return VisitNe((QilBinary)n);
                case QilNodeType.Eq: return VisitEq((QilBinary)n);
                case QilNodeType.Gt: return VisitGt((QilBinary)n);
                case QilNodeType.Ge: return VisitGe((QilBinary)n);
                case QilNodeType.Lt: return VisitLt((QilBinary)n);
                case QilNodeType.Le: return VisitLe((QilBinary)n);
                
                case QilNodeType.Is: return VisitIs((QilBinary)n);
                case QilNodeType.After: return VisitAfter((QilBinary)n);
                case QilNodeType.Before: return VisitBefore((QilBinary)n);
                
                case QilNodeType.Loop: return VisitLoop((QilLoop)n);
                case QilNodeType.Filter: return VisitFilter((QilLoop)n);
                
                case QilNodeType.Sort: return VisitSort((QilLoop)n);
                case QilNodeType.SortKey: return VisitSortKey((QilSortKey)n);
                case QilNodeType.DocOrderDistinct: return VisitDocOrderDistinct((QilUnary)n);
                
                case QilNodeType.Function: return VisitFunction((QilFunction)n);
                case QilNodeType.Invoke: return VisitInvoke((QilInvoke)n);
                
                case QilNodeType.Content: return VisitContent((QilUnary)n);
                case QilNodeType.Attribute: return VisitAttribute((QilBinary)n);
                case QilNodeType.Parent: return VisitParent((QilUnary)n);
                case QilNodeType.Root: return VisitRoot((QilUnary)n);
                case QilNodeType.XmlContext: return VisitXmlContext(n);
                case QilNodeType.Descendant: return VisitDescendant((QilUnary)n);
                case QilNodeType.DescendantOrSelf: return VisitDescendantOrSelf((QilUnary)n);
                case QilNodeType.Ancestor: return VisitAncestor((QilUnary)n);
                case QilNodeType.AncestorOrSelf: return VisitAncestorOrSelf((QilUnary)n);
                case QilNodeType.Preceding: return VisitPreceding((QilUnary)n);
                case QilNodeType.FollowingSibling: return VisitFollowingSibling((QilUnary)n);
                case QilNodeType.PrecedingSibling: return VisitPrecedingSibling((QilUnary)n);
                case QilNodeType.NodeRange: return VisitNodeRange((QilBinary)n);
                case QilNodeType.Deref: return VisitDeref((QilBinary)n);
                
                case QilNodeType.ElementCtor: return VisitElementCtor((QilBinary)n);
                case QilNodeType.AttributeCtor: return VisitAttributeCtor((QilBinary)n);
                case QilNodeType.CommentCtor: return VisitCommentCtor((QilUnary)n);
                case QilNodeType.PICtor: return VisitPICtor((QilBinary)n);
                case QilNodeType.TextCtor: return VisitTextCtor((QilUnary)n);
                case QilNodeType.RawTextCtor: return VisitRawTextCtor((QilUnary)n);
                case QilNodeType.DocumentCtor: return VisitDocumentCtor((QilUnary)n);
                case QilNodeType.NamespaceDecl: return VisitNamespaceDecl((QilBinary)n);
                case QilNodeType.RtfCtor: return VisitRtfCtor((QilBinary)n);
                
                case QilNodeType.NameOf: return VisitNameOf((QilUnary)n);
                case QilNodeType.LocalNameOf: return VisitLocalNameOf((QilUnary)n);
                case QilNodeType.NamespaceUriOf: return VisitNamespaceUriOf((QilUnary)n);
                case QilNodeType.PrefixOf: return VisitPrefixOf((QilUnary)n);
                
                case QilNodeType.TypeAssert: return VisitTypeAssert((QilTargetType)n);
                case QilNodeType.IsType: return VisitIsType((QilTargetType)n);
                case QilNodeType.IsEmpty: return VisitIsEmpty((QilUnary)n);
                
                case QilNodeType.XPathNodeValue: return VisitXPathNodeValue((QilUnary)n);
                case QilNodeType.XPathFollowing: return VisitXPathFollowing((QilUnary)n);
                case QilNodeType.XPathPreceding: return VisitXPathPreceding((QilUnary)n);
                case QilNodeType.XPathNamespace: return VisitXPathNamespace((QilUnary)n);
                
                case QilNodeType.XsltGenerateId: return VisitXsltGenerateId((QilUnary)n);
                case QilNodeType.XsltInvokeLateBound: return VisitXsltInvokeLateBound((QilInvokeLateBound)n);
                case QilNodeType.XsltInvokeEarlyBound: return VisitXsltInvokeEarlyBound((QilInvokeEarlyBound)n);
                case QilNodeType.XsltCopy: return VisitXsltCopy((QilBinary)n);
                case QilNodeType.XsltCopyOf: return VisitXsltCopyOf((QilUnary)n);
                case QilNodeType.XsltConvert: return VisitXsltConvert((QilTargetType)n);
                
                default: return VisitUnknown(n);
            }
        }
        
        protected virtual QilNode VisitReference(QilNode n) {
            if (n == null)
                return VisitNull();
            
            switch (n.NodeType) {
                case QilNodeType.For: return VisitForReference((QilIterator)n);
                case QilNodeType.Let: return VisitLetReference((QilIterator)n);
                case QilNodeType.Parameter: return VisitParameterReference((QilParameter)n);
                
                case QilNodeType.Function: return VisitFunctionReference((QilFunction)n);
                
                default: return VisitUnknown(n);
            }
        }
        
        protected virtual QilNode VisitNull() { return null; }
        
        #region meta
        protected virtual QilNode VisitQilExpression(QilExpression n) { return VisitChildren(n); }
        protected virtual QilNode VisitFunctionList(QilList n) { return VisitChildren(n); }
        protected virtual QilNode VisitGlobalVariableList(QilList n) { return VisitChildren(n); }
        protected virtual QilNode VisitGlobalParameterList(QilList n) { return VisitChildren(n); }
        protected virtual QilNode VisitActualParameterList(QilList n) { return VisitChildren(n); }
        protected virtual QilNode VisitFormalParameterList(QilList n) { return VisitChildren(n); }
        protected virtual QilNode VisitSortKeyList(QilList n) { return VisitChildren(n); }
        protected virtual QilNode VisitBranchList(QilList n) { return VisitChildren(n); }
        protected virtual QilNode VisitOptimizeBarrier(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitUnknown(QilNode n) { return VisitChildren(n); }
        #endregion
        
        #region specials
        protected virtual QilNode VisitDataSource(QilDataSource n) { return VisitChildren(n); }
        protected virtual QilNode VisitNop(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitError(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitWarning(QilUnary n) { return VisitChildren(n); }
        #endregion
        
        #region variables
        protected virtual QilNode VisitFor(QilIterator n) { return VisitChildren(n); }
        protected virtual QilNode VisitForReference(QilIterator n) { return n; }
        protected virtual QilNode VisitLet(QilIterator n) { return VisitChildren(n); }
        protected virtual QilNode VisitLetReference(QilIterator n) { return n; }
        protected virtual QilNode VisitParameter(QilParameter n) { return VisitChildren(n); }
        protected virtual QilNode VisitParameterReference(QilParameter n) { return n; }
        protected virtual QilNode VisitPositionOf(QilUnary n) { return VisitChildren(n); }
        #endregion
        
        #region literals
        protected virtual QilNode VisitTrue(QilNode n) { return VisitChildren(n); }
        protected virtual QilNode VisitFalse(QilNode n) { return VisitChildren(n); }
        protected virtual QilNode VisitLiteralString(QilLiteral n) { return VisitChildren(n); }
        protected virtual QilNode VisitLiteralInt32(QilLiteral n) { return VisitChildren(n); }
        protected virtual QilNode VisitLiteralInt64(QilLiteral n) { return VisitChildren(n); }
        protected virtual QilNode VisitLiteralDouble(QilLiteral n) { return VisitChildren(n); }
        protected virtual QilNode VisitLiteralDecimal(QilLiteral n) { return VisitChildren(n); }
        protected virtual QilNode VisitLiteralQName(QilName n) { return VisitChildren(n); }
        protected virtual QilNode VisitLiteralType(QilLiteral n) { return VisitChildren(n); }
        protected virtual QilNode VisitLiteralObject(QilLiteral n) { return VisitChildren(n); }
        #endregion
        
        #region boolean operators
        protected virtual QilNode VisitAnd(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitOr(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitNot(QilUnary n) { return VisitChildren(n); }
        #endregion
        
        #region choice
        protected virtual QilNode VisitConditional(QilTernary n) { return VisitChildren(n); }
        protected virtual QilNode VisitChoice(QilChoice n) { return VisitChildren(n); }
        #endregion
        
        #region collection operators
        protected virtual QilNode VisitLength(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitSequence(QilList n) { return VisitChildren(n); }
        protected virtual QilNode VisitUnion(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitIntersection(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitDifference(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitAverage(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitSum(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitMinimum(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitMaximum(QilUnary n) { return VisitChildren(n); }
        #endregion
        
        #region arithmetic operators
        protected virtual QilNode VisitNegate(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitAdd(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitSubtract(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitMultiply(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitDivide(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitModulo(QilBinary n) { return VisitChildren(n); }
        #endregion
        
        #region string operators
        protected virtual QilNode VisitStrLength(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitStrConcat(QilStrConcat n) { return VisitChildren(n); }
        protected virtual QilNode VisitStrParseQName(QilBinary n) { return VisitChildren(n); }
        #endregion
        
        #region value comparison operators
        protected virtual QilNode VisitNe(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitEq(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitGt(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitGe(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitLt(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitLe(QilBinary n) { return VisitChildren(n); }
        #endregion
        
        #region node comparison operators
        protected virtual QilNode VisitIs(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitAfter(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitBefore(QilBinary n) { return VisitChildren(n); }
        #endregion
        
        #region loops
        protected virtual QilNode VisitLoop(QilLoop n) { return VisitChildren(n); }
        protected virtual QilNode VisitFilter(QilLoop n) { return VisitChildren(n); }
        #endregion
        
        #region sorting
        protected virtual QilNode VisitSort(QilLoop n) { return VisitChildren(n); }
        protected virtual QilNode VisitSortKey(QilSortKey n) { return VisitChildren(n); }
        protected virtual QilNode VisitDocOrderDistinct(QilUnary n) { return VisitChildren(n); }
        #endregion
        
        #region function definition and invocation
        protected virtual QilNode VisitFunction(QilFunction n) { return VisitChildren(n); }
        protected virtual QilNode VisitFunctionReference(QilFunction n) { return n; }
        protected virtual QilNode VisitInvoke(QilInvoke n) { return VisitChildren(n); }
        #endregion
        
        #region XML navigation
        protected virtual QilNode VisitContent(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitAttribute(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitParent(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitRoot(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitXmlContext(QilNode n) { return VisitChildren(n); }
        protected virtual QilNode VisitDescendant(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitDescendantOrSelf(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitAncestor(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitAncestorOrSelf(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitPreceding(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitFollowingSibling(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitPrecedingSibling(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitNodeRange(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitDeref(QilBinary n) { return VisitChildren(n); }
        #endregion
        
        #region XML construction
        protected virtual QilNode VisitElementCtor(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitAttributeCtor(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitCommentCtor(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitPICtor(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitTextCtor(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitRawTextCtor(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitDocumentCtor(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitNamespaceDecl(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitRtfCtor(QilBinary n) { return VisitChildren(n); }
        #endregion
        
        #region Node properties
        protected virtual QilNode VisitNameOf(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitLocalNameOf(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitNamespaceUriOf(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitPrefixOf(QilUnary n) { return VisitChildren(n); }
        #endregion
        
        #region Type operators
        protected virtual QilNode VisitTypeAssert(QilTargetType n) { return VisitChildren(n); }
        protected virtual QilNode VisitIsType(QilTargetType n) { return VisitChildren(n); }
        protected virtual QilNode VisitIsEmpty(QilUnary n) { return VisitChildren(n); }
        #endregion
        
        #region XPath operators
        protected virtual QilNode VisitXPathNodeValue(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitXPathFollowing(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitXPathPreceding(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitXPathNamespace(QilUnary n) { return VisitChildren(n); }
        #endregion
        
        #region XSLT
        protected virtual QilNode VisitXsltGenerateId(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitXsltInvokeLateBound(QilInvokeLateBound n) { return VisitChildren(n); }
        protected virtual QilNode VisitXsltInvokeEarlyBound(QilInvokeEarlyBound n) { return VisitChildren(n); }
        protected virtual QilNode VisitXsltCopy(QilBinary n) { return VisitChildren(n); }
        protected virtual QilNode VisitXsltCopyOf(QilUnary n) { return VisitChildren(n); }
        protected virtual QilNode VisitXsltConvert(QilTargetType n) { return VisitChildren(n); }
        #endregion
        
        #endregion // AUTOGENERATED
    }

    //for checking the depth of QilNode to avoid StackOverflow in visit stage
    internal class QilDepthChecker {
        const int MAX_QIL_DEPTH = 800;
        private Dictionary<QilNode, bool> visitedRef = new Dictionary<QilNode, bool>();

        public static void Check(QilNode input) {
            if (System.Xml.XmlConfiguration.XsltConfigSection.LimitXPathComplexity) {
                new QilDepthChecker().Check(input, 0);
            }
        }
        
        private void Check(QilNode input, int depth) {
            if (depth > MAX_QIL_DEPTH) {
                throw XsltException.Create(System.Xml.Utils.Res.Xslt_InputTooComplex);
            }
            //QilReference node may duplicate, the first one is definition and should expand, others are reference.
            if (input is QilReference) {
                if (visitedRef.ContainsKey(input))
                    return;
                visitedRef[input] = true;
            }
            int nextDepth = depth + 1;
            for (int i = 0; i < input.Count; i++) {
                QilNode child = input[i];
                if (child != null) {
                    Check(child, nextDepth);
                }
            }
        }
    }
}
