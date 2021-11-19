using System.Collections.Generic;

namespace CollisionLODOverride
{
    public static class PathSets
    {
        public static readonly string[] blackbeachPaths = new string[]
        {
            "FOLIAGE/spmBbConif (13)",
            "FOLIAGE/spmBbConif (3)",
            "FOLIAGE/spmBbConif (11)",
            "FOLIAGE/spmBbConif (21)",
            "FOLIAGE/spmBbConif (18)",
            "FOLIAGE/spmBbConif (6)",
            "SKYBOX/ClosePillar/spmBbConif (7)",
            "FOLIAGE/spmBbConif (10)",
            "FOLIAGE/spmBbConif (4)",
            "FOLIAGE/spmBbConif (12)",
            "FOLIAGE/spmBbConif (8)",
            "FOLIAGE/spmBbConif (7)",
            "SKYBOX/DistantPillar/spmBbConif (6)",
            "FOLIAGE/spmBbConif (2)",
            "FOLIAGE/spmBbConif (15)",
            "FOLIAGE/spmBbConif (5)",
            "FOLIAGE/spmBbConif (20)",
            "FOLIAGE/spmBbConif (19)",
            "FOLIAGE/spmBbConif (22)",
            "FOLIAGE/spmBbConif (9)",
            "FOLIAGE/spmBbConif (14)",
            "FOLIAGE/spmBbConif (7)",
            "FOLIAGE/spmBbConif (17)",
            "FOLIAGE/spmBbConif (8)",
            "FOLIAGE/spmBbConif (16)",
            "FOLIAGE/spmBbConif",
            "SKYBOX/ClosePillar/spmBbConif (8)",
            "FOLIAGE/spmBbConif (9)"
        };

        public static readonly string[] foggyswampPaths = new string[]
        {
            "HOLDER: Skybox/SkyboxTrees/FSTreeTrunkEnormousNoCollisionSkybox/spmBbConif (1)",
            "HOLDER: Skybox/SkyboxTrees/FSTreeTrunkEnormousNoCollisionSkybox/spmBbConif",
            "HOLDER: Skybox/SkyboxTrees/FSTreeTrunkEnormousNoCollisionSkybox (2)/spmBbConif (1)",
            "HOLDER: Root Bundles/FSRootBundleLargeCollision (1)",
            "HOLDER: Root Bundles/FSRootBundleLargeCollision (5)",
            "HOLDER: Root Bundles/FSRootBundleLargeCollision (4)",
            "HOLDER: Root Bundles/FSRootBundleLargeCollision (8)",
            "HOLDER: Root Bundles/FSRootBundleLargeCollision",
            "HOLDER: Tree Trunks w Collision/CompositeTreeTrunk/FSRootBundleLargeCollision (1)",
            "HOLDER: Hidden Altar Stuff/Foliage/FSRootBundleLargeCollision (9)",
            "HOLDER: Tree Trunks w Collision/FSTreeTrunkLong (1)/FSRootBundleLargeCollision (1)",
            "HOLDER: Root Bundles/FSRootBundleLargeCollision (3)",
            "HOLDER: Hidden Altar Stuff/Foliage/FSRootBundleLargeCollision (10)",
            "HOLDER: Root Bundles/FSRootBundleLargeCollision (2)",
            "HOLDER: Root Bundles/FSRootBundleLargeCollision (10)",
            "HOLDER: Root Bundles/FSRootBundleLargeCollision (6)",
            "HOLDER: Root Bundles/FSRootBundleLargeCollision",
            "HOLDER: Root Bundles/FSRootBundleLargeCollision (7)",
            "HOLDER: Hidden Altar Stuff/Foliage/FSRootBundleSmallCollision (3)",
            "HOLDER: Hidden Altar Stuff/Foliage/FSRootBundleSmallCollision (2)",
            "HOLDER: Root Bundles/FSRootBundleSmallCollision",
            "HOLDER: Hidden Altar Stuff/Foliage/FSRootBundleSmallCollision (5)",
            "HOLDER: Root Bundles/FSRootBundleSmallCollision",
            "HOLDER: Hidden Altar Stuff/Foliage/FSRootBundleSmallCollision (4)",
            "HOLDER: Root Bundles/FSRootBundleSmallCollision (1)"
        };


        public static Dictionary<string, string[]> sceneName_to_pathSets = new Dictionary<string, string[]>()
        {
            { "blackbeach", blackbeachPaths },
            { "foggyswamp", foggyswampPaths }
        };
    }
}