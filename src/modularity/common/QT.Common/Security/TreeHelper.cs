using NPOI.POIFS.Properties;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.FriendlyException;

namespace QT.Common.Security;

/// <summary>
/// 树结构帮助类.
/// </summary>
[SuppressSniffer]
public static class TreeHelper
{
    /// <summary>
    /// 建造树结构.
    /// </summary>
    /// <param name="allNodes">所有的节点.</param>
    /// <param name="parentId">节点.</param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this List<T> allNodes, string parentId = "0")
        where T : ITreeModel, new()
    {
        List<T> resData = new List<T>();

        // 查找出父类对象
        List<T> rootNodes = allNodes.FindAll(x => x.parentId == parentId || x.parentId.IsNullOrEmpty());

        // 移除父类对象
        allNodes.RemoveAll(x => x.parentId == parentId || x.parentId.IsNullOrEmpty());
        resData = rootNodes;
        resData.ForEach(aRootNode =>
        {
            aRootNode.hasChildren = HaveChildren(allNodes, aRootNode.id);
            if (aRootNode.hasChildren)
            {
                aRootNode.children = _GetChildren(allNodes, aRootNode);
                aRootNode.num = aRootNode.children.Count();
            }
            else
            {
                aRootNode.isLeaf = !aRootNode.hasChildren;
                aRootNode.children = null;
            }
        });
        return resData;
    }

    /// <summary>
    /// 建造树结构.
    /// </summary>
    /// <param name="allNodes">所有的节点.</param>
    /// <param name="parentId">节点.</param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this List<T> allNodes, Func<T, string> idSelector, string parentId = "0")
        where T : ITreeModelFun, new()
    {
        List<T> resData = new List<T>();

        // 查找出父类对象 

        //if (parentSelector == null)
        //{
        //    parentSelector= x=> x.GetParentId() == parentId || x.parentId.IsNullOrEmpty()
        //}

        List<T> rootNodes = allNodes.FindAll(x => x.GetParentId() == parentId || x.GetParentId().IsNullOrEmpty());

        // 移除父类对象
        allNodes.RemoveAll(x => x.GetParentId() == parentId || x.GetParentId().IsNullOrEmpty());

        int loopCount = 0;
        Func<List<T>, string, bool> _hasChildren = (nodes, nodeId) => nodes.Exists(x => x.GetParentId() == nodeId);

        Func<List<T>, string, List<object>> _getChildren = null;
        _getChildren = (nodes, parentNodeId) =>
        {
            loopCount++;
            if (loopCount > 100000)
            {
                throw Oops.Oh("TreeHelper...ToTree...异常");
            }
            Type type = typeof(T);
            var properties = type.GetProperties().ToList();
            List<object> resData = new List<object>();

            // 查找出父类对象
            var children = nodes.FindAll(x => x.GetParentId() == parentNodeId);

            // 移除父类对象
            nodes.RemoveAll(x => x.GetParentId() == parentNodeId);
            children.ForEach(aChildren =>
            {
                T newNode = new T();
                resData.Add(newNode);

                // 赋值属性
                foreach (var aProperty in properties.Where(x => x.CanWrite))
                {
                    var value = aProperty.GetValue(aChildren, null);
                    aProperty.SetValue(newNode, value);
                }

                newNode.hasChildren = _hasChildren(nodes, idSelector(aChildren));
                if (newNode.hasChildren)
                {
                    newNode.children = _getChildren(nodes, idSelector(newNode));
                }
                else
                {
                    newNode.isLeaf = !newNode.hasChildren;
                    newNode.children = null;
                }
            });
            return resData;
        };

        resData = rootNodes;
        resData.ForEach(aRootNode =>
        {
            aRootNode.hasChildren = _hasChildren(allNodes, idSelector(aRootNode)); // HaveChildren(allNodes, aRootNode.id);
            if (aRootNode.hasChildren)
            {
                aRootNode.children = _getChildren(allNodes, idSelector(aRootNode));
                aRootNode.num = aRootNode.children.Count();
            }
            else
            {
                aRootNode.isLeaf = !aRootNode.hasChildren;
                aRootNode.children = null;
            }
        });
        return resData;
    }

    #region 私有成员

    /// <summary>
    /// 获取所有子节点.
    /// </summary>
    /// <typeparam name="T">树模型（TreeModel或继承它的模型.</typeparam>
    /// <param name="nodes">所有节点列表.</param>
    /// <param name="parentNode">父节点Id.</param>
    /// <returns></returns>
    private static List<object> _GetChildren<T>(List<T> nodes, T parentNode)
        where T : ITreeModel, new()
    {
        Type type = typeof(T);
        var properties = type.GetProperties().ToList();
        List<object> resData = new List<object>();

        // 查找出父类对象
        var children = nodes.FindAll(x => x.parentId == parentNode.id);

        // 移除父类对象
        nodes.RemoveAll(x => x.parentId == parentNode.id);
        children.ForEach(aChildren =>
        {
            T newNode = new T();
            resData.Add(newNode);

            // 赋值属性
            foreach (var aProperty in properties.Where(x => x.CanWrite))
            {
                var value = aProperty.GetValue(aChildren, null);
                aProperty.SetValue(newNode, value);
            }

            newNode.hasChildren = HaveChildren(nodes, aChildren.id);
            if (newNode.hasChildren)
            {
                newNode.children = _GetChildren(nodes, newNode);
            }
            else
            {
                newNode.isLeaf = !newNode.hasChildren;
                newNode.children = null;
            }
        });
        return resData;
    }

    /// <summary>
    /// 判断当前节点是否有子节点.
    /// </summary>
    /// <typeparam name="T">树模型.</typeparam>
    /// <param name="nodes">所有节点.</param>
    /// <param name="nodeId">当前节点Id.</param>
    /// <returns></returns>
    private static bool HaveChildren<T>(List<T> nodes, string nodeId)
        where T : ITreeModel, new()
    {
        return nodes.Exists(x => x.parentId == nodeId);
    }
    #endregion
}

/// <summary>
/// 树模型基类.
/// </summary>
public class TreeModel : ITreeModel
{
    /// <summary>
    /// 获取节点id.
    /// </summary>
    /// <returns></returns>
    public string id { get; set; }

    /// <summary>
    /// 获取节点父id.
    /// </summary>
    /// <returns></returns>
    public string parentId { get; set; }

    /// <summary>
    /// 是否有子级.
    /// </summary>
    public bool hasChildren { get; set; }

    /// <summary>
    /// 设置Children.
    /// </summary>
    public List<object>? children { get; set; } = new List<object>();

    /// <summary>
    /// 子节点数量.
    /// </summary>
    public int num { get; set; }

    /// <summary>
    /// 是否为子节点.
    /// </summary>
    public bool isLeaf { get; set; } = false;
}

/// <summary>
/// 树模型基类.
/// </summary>
public interface ITreeModel
{
    /// <summary>
    /// 获取节点id.
    /// </summary>
    /// <returns></returns>
    public string id { get; set; }
    //string GetId();

    /// <summary>
    /// 获取节点父id.
    /// </summary>
    /// <returns></returns>
    public string parentId { get; set; }
    //string GetParentId();

    /// <summary>
    /// 是否有子级.
    /// </summary>
    public bool hasChildren { get; set; }

    /// <summary>
    /// 设置Children.
    /// </summary>
    public List<object>? children { get; set; }

    /// <summary>
    /// 子节点数量.
    /// </summary>
    public int num { get; set; }

    /// <summary>
    /// 是否为子节点.
    /// </summary>
    public bool isLeaf { get; set; }
}

/// <summary>
/// 树模型基类.
/// </summary>
public interface ITreeModelFun
{
    /// <summary>
    /// 获取节点id.
    /// </summary>
    /// <returns></returns>
    //public string id { get; set; }
    string GetId();

    /// <summary>
    /// 获取节点父id.
    /// </summary>
    /// <returns></returns>
    //public string parentId { get; set; }
    string GetParentId();

    /// <summary>
    /// 是否有子级.
    /// </summary>
    public bool hasChildren { get; set; }

    /// <summary>
    /// 设置Children.
    /// </summary>
    public List<object>? children { get; set; }

    /// <summary>
    /// 子节点数量.
    /// </summary>
    public int num { get; set; }

    /// <summary>
    /// 是否为子节点.
    /// </summary>
    public bool isLeaf { get; set; }
}