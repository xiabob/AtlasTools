using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//要使用SpriteAtlas，需要引入UnityEngine.U2D
using UnityEngine.U2D;
using UnityEngine.UI;

public class AtlasUsage : MonoBehaviour
{
    //给SpriteAtlas赋值，要么“连线”，要么通过Resources来load
    [SerializeField] private SpriteAtlas _carAtlas;
    private SpriteAtlas _resAtlas;

    [SerializeField] private Image _carImage;
    [SerializeField] private Image _resImage;

	private Sprite[] _carSprites;

    // Use this for initialization
    void Start()
    {
        //通过Resources来load
        _resAtlas = Resources.Load<SpriteAtlas>("ResAtlas");

        //拿到atlas所有的sprite
		_carSprites = new Sprite[_carAtlas.spriteCount];
		_carAtlas.GetSprites(_carSprites);

		StartCoroutine(RandomDisply());
    }

    private IEnumerator RandomDisply()
    {
        while (true)
        {
			int carIndex = Random.Range(0, _carSprites.Length);
			print(carIndex);
			_carImage.sprite = _carSprites[carIndex];

			int resIndex = Random.Range(1, 5);
            //或者通过名称直接获取atlas对应的sprite
			_resImage.sprite = _resAtlas.GetSprite(resIndex.ToString());

            yield return new WaitForSeconds(2);
        }
    }
}
